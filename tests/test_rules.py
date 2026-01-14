import uuid
import pytest
from tests.helpers.assertions import assert_status, assert_problem_details

pytestmark = pytest.mark.integration


def _rules_search(authed, query=None):
    """
    ApiClient.rules_search() in this test harness does NOT accept kwargs.
    It expects a single dict of query params (or None).
    """
    if query is None:
        return authed.rules_search()
    return authed.rules_search(query)


def _get_any_rule(authed):
    """
    Fetch one rule from GET /api/rules and return the first item as a dict.
    Assumes response shape: { items: [...], page, pageSize, total, totalPages }.
    """
    r = _rules_search(authed, {"page": 1, "pageSize": 1})
    assert_status(r, 200)
    body = r.json()
    assert "items" in body and isinstance(body["items"], list) and body["items"], "Expected at least one seeded rule"
    return body["items"][0]


def _invalid_parameters_for_rule_type(rule_type: str):
    """
    Produce deterministic INVALID parameter payloads intended to fail the
    Application parameter validator for common rule types.
    """
    if rule_type == "DailyLimit":
        return {"limitBaseAmount": 0}
    if rule_type == "Structuring":
        return {"n": 0, "xBaseAmount": 0}
    if rule_type == "BannedCountries":
        return {"countries": "US"}  # should be array
    if rule_type == "BannedAccounts":
        return {"entries": "not-an-array"}  # should be array

    # Unknown / future rule types: empty object should fail if required fields exist
    return {}


# ----------------------------
# SEARCH (GET /api/rules)
# ----------------------------

def test_rules_search_returns_paged_response(authed, api_up):
    r = _rules_search(authed)
    assert_status(r, 200)
    body = r.json()

    assert isinstance(body.get("items"), list)
    assert isinstance(body.get("page"), int)
    assert isinstance(body.get("pageSize"), int)
    assert isinstance(body.get("total"), int)
    assert isinstance(body.get("totalPages"), int)


@pytest.mark.parametrize(
    "query",
    [
        {"page": 0},
        {"pageSize": 0},
        {"pageSize": 101},  # expects strict MaxPageSize=100 validation
    ],
)
def test_rules_search_invalid_pagination_rejected(authed, api_up, query):
    r = _rules_search(authed, query)
    assert_problem_details(r, 400)


@pytest.mark.parametrize(
    "query",
    [
        {"sortBy": "NOT_A_FIELD"},
        {"sortDir": "sideways"},
    ],
)
def test_rules_search_invalid_sort_rejected(authed, api_up, query):
    r = _rules_search(authed, query)
    assert_problem_details(r, 400)


def test_rules_search_requires_auth(api, api_up):
    r = api.rules_search()
    assert r.status_code == 401


# ----------------------------
# GET BY ID (GET /api/rules/{id})
# ----------------------------

def test_rule_get_by_id_success(authed, api_up):
    rule = _get_any_rule(authed)
    rule_id = rule["id"]

    r = authed.rules_get(rule_id)
    assert_status(r, 200)

    body = r.json()
    assert body["id"] == rule_id
    assert "code" in body
    assert "ruleType" in body
    assert "parameters" in body


def test_rule_get_by_id_not_found_returns_404_problem_details(authed, api_up):
    r = authed.rules_get("00000000-0000-0000-0000-000000000000")
    assert_problem_details(r, 404)


def test_rule_get_by_id_requires_auth(api, api_up):
    r = api.rules_get("00000000-0000-0000-0000-000000000000")
    assert r.status_code == 401


# ----------------------------
# PATCH (PATCH /api/rules/{id})
# Cover: Success, NotFound, InvalidParameters, InvalidUpdate, NoChanges
# ----------------------------

def test_patch_rule_not_found_returns_404_problem_details(authed, api_up):
    r = authed.rules_patch("00000000-0000-0000-0000-000000000000", {"name": "X"})
    assert_problem_details(r, 404)


def test_patch_rule_requires_auth(api, api_up):
    r = api.rules_patch("00000000-0000-0000-0000-000000000000", {"name": "X"})
    assert r.status_code == 401


def test_patch_rule_empty_object_rejected_by_validation(authed, api_up):
    """
    With 'hasAny' moved to FluentValidation, {} should be rejected with 400.
    """
    rule = _get_any_rule(authed)
    r = authed.rules_patch(rule["id"], {})
    assert_problem_details(r, 400)


def test_patch_rule_no_effective_change_returns_no_changes(authed, api_up):
    """
    This is the Application-level 'NoChanges' outcome:
    valid patch payload, but results in no persisted change.
    This test will PASS only after you implement actual-change detection in the service.
    """
    rule = _get_any_rule(authed)
    rule_id = rule["id"]

    # no-op: same name
    r = authed.rules_patch(rule_id, {"name": rule["name"]})
    assert_problem_details(r, 400)


def test_patch_rule_invalid_parameters_returns_400(authed, api_up):
    rule = _get_any_rule(authed)
    rule_id = rule["id"]
    rule_type = rule["ruleType"]

    invalid_params = _invalid_parameters_for_rule_type(rule_type)
    r = authed.rules_patch(rule_id, {"parameters": invalid_params})
    assert_problem_details(r, 400)


def test_patch_rule_invalid_update_returns_400(authed, api_up):
    """
    May fail either in FluentValidation or domain logic; both should be 400.
    """
    rule = _get_any_rule(authed)
    r = authed.rules_patch(rule["id"], {"name": "   "})
    assert_problem_details(r, 400)


def test_patch_rule_success_update_and_revert_name(authed, api_up):
    rule = _get_any_rule(authed)
    rule_id = rule["id"]
    original_name = rule["name"]

    new_name = f"{original_name} - it-{uuid.uuid4().hex[:8]}"

    r1 = authed.rules_patch(rule_id, {"name": new_name})
    assert_status(r1, 200)
    assert r1.json()["name"] == new_name

    r2 = authed.rules_patch(rule_id, {"name": original_name})
    assert_status(r2, 200)
    assert r2.json()["name"] == original_name
