import uuid
import pytest

from tests.helpers.assertions import assert_status, assert_problem_details

pytestmark = pytest.mark.integration


# If your API validates these strictly (recommended), invalid values must return 400 ProblemDetails.
INVALID_SORT_BY = "NotAField"
INVALID_SORT_DIR = "sideways"
INVALID_SCOPE = "InvalidScope"


def _is_rule_type(value, expected_name: str, expected_numeric: int | None = None) -> bool:
    """
    Support both enum serialization modes:
      - string enums (e.g., "DailyLimit")
      - numeric enums (e.g., 0)
    """
    if isinstance(value, str):
        return value.strip().lower() == expected_name.strip().lower()
    if isinstance(value, int) and expected_numeric is not None:
        return value == expected_numeric
    return False


def _pick_rule(items: list[dict], *, rule_type_name: str, rule_type_numeric: int | None = None) -> dict | None:
    for it in items:
        if _is_rule_type(it.get("ruleType"), rule_type_name, rule_type_numeric):
            return it
    return None


def _list_rules(authed, **params) -> dict:
    r = authed.rules_search(params=params)
    assert_status(r, 200)
    body = r.json()
    if not isinstance(body, dict):
        raise AssertionError(f"Expected JSON object, got: {body}")
    return body


def _assert_paged_response_shape(body: dict):
    """
    Validate the common paged response shape.
    This expects the API to serialize properties to camelCase (default in ASP.NET).
    """
    for k in ("items", "page", "pageSize", "total", "totalPages"):
        if k not in body:
            raise AssertionError(f"Missing '{k}' in paged response: {body}")

    if not isinstance(body["items"], list):
        raise AssertionError(f"Expected 'items' to be a list, got: {type(body['items'])}")

    if not isinstance(body["page"], int) or body["page"] < 1:
        raise AssertionError(f"Expected 'page' to be int >= 1, got: {body['page']}")

    if not isinstance(body["pageSize"], int) or body["pageSize"] < 1:
        raise AssertionError(f"Expected 'pageSize' to be int >= 1, got: {body['pageSize']}")

    if not isinstance(body["total"], int) or body["total"] < 0:
        raise AssertionError(f"Expected 'total' to be int >= 0, got: {body['total']}")

    if not isinstance(body["totalPages"], int) or body["totalPages"] < 0:
        raise AssertionError(f"Expected 'totalPages' to be int >= 0, got: {body['totalPages']}")


def _assert_rule_dto_shape(item: dict):
    """
    Validate key fields for a ComplianceRuleDto.
    Keep this tolerant to DTO evolution, but enforce critical invariants.
    """
    required = ["id", "ruleType", "name", "isActive", "severity", "parameters", "createdAtUtc", "updatedAtUtc"]
    for k in required:
        if k not in item:
            raise AssertionError(f"Rule item missing '{k}': {item}")

    # id should look like a UUID string
    try:
        uuid.UUID(item["id"])
    except Exception as ex:
        raise AssertionError(f"Rule 'id' is not a valid UUID: {item.get('id')}") from ex

    if not isinstance(item["name"], str) or not item["name"].strip():
        raise AssertionError(f"Rule 'name' must be a non-empty string. Item={item}")

    if not isinstance(item["isActive"], bool):
        raise AssertionError(f"Rule 'isActive' must be boolean. Item={item}")

    # parameters should be JSON object (dict) typically
    if item["parameters"] is None or not isinstance(item["parameters"], (dict, list)):
        # Allow list for future shapes, but disallow scalar/null
        raise AssertionError(f"Rule 'parameters' must be json object/array. Item={item}")


def test_rules_search_requires_auth(api, api_up):
    r = api.rules_search()
    assert r.status_code == 401


def test_rules_get_requires_auth(api, api_up):
    r = api.rules_get(str(uuid.uuid4()))
    assert r.status_code == 401


def test_rules_patch_requires_auth(api, api_up):
    r = api.rules_patch(str(uuid.uuid4()), payload={"name": "x"})
    assert r.status_code == 401


def test_rules_search_ok_default(authed, api_up):
    body = _list_rules(authed)
    _assert_paged_response_shape(body)

    # Validate returned items (if any)
    for it in body["items"]:
        _assert_rule_dto_shape(it)


def test_rules_search_page_size_cap(authed, api_up):
    # If you cap pageSize (recommended), assert it never returns absurd values.
    body = _list_rules(authed, page=1, pageSize=1000)
    _assert_paged_response_shape(body)
    assert body["pageSize"] <= 100


def test_rules_search_invalid_sort_by(authed, api_up):
    r = authed.rules_search(
        params={"sortBy": INVALID_SORT_BY, "sortDir": "asc"}
    )
    assert_problem_details(r, 400)


def test_rules_search_invalid_sort_dir(authed, api_up):
    r = authed.rules_search(
        params={"sortBy": "UpdatedAtUtc", "sortDir": INVALID_SORT_DIR}
    )
    assert_problem_details(r, 400)


def test_rules_search_invalid_scope(authed, api_up):
    r = authed.rules_search(
        params={"scope": INVALID_SCOPE}
    )
    assert_problem_details(r, 400)


def test_rules_get_not_found(authed, api_up):
    r = authed.rules_get(str(uuid.uuid4()))
    assert_problem_details(r, 404)


def test_rules_get_by_id_ok(authed, api_up):
    body = _list_rules(authed, page=1, pageSize=10)
    _assert_paged_response_shape(body)

    if not body["items"]:
        pytest.skip("No rules seeded; cannot test GET by id.")

    rule = body["items"][0]
    rule_id = rule["id"]

    r = authed.rules_get(rule_id)
    assert_status(r, 200)
    item = r.json()
    _assert_rule_dto_shape(item)
    assert item["id"] == rule_id


def test_rules_patch_no_body_invalid_payload(authed, api_up):
    # No JSON at all -> controller should reject as invalid payload (400).
    # This depends on your model-binding behavior; best practice is 400 ProblemDetails.
    body = _list_rules(authed, page=1, pageSize=1)
    if not body["items"]:
        pytest.skip("No rules seeded; cannot test PATCH.")
    rule_id = body["items"][0]["id"]

    r = authed.rules_patch(rule_id, payload=None)
    # Depending on your setup, ASP.NET might return 415 or 400.
    # If you standardized on ProblemDetails, expect 400.
    assert r.status_code in (400, 415)
    if r.status_code == 400:
        assert_problem_details(r, 400)


def test_rules_patch_no_changes(authed, api_up):
    body = _list_rules(authed, page=1, pageSize=1)
    if not body["items"]:
        pytest.skip("No rules seeded; cannot test PATCH.")
    rule_id = body["items"][0]["id"]

    r = authed.rules_patch(rule_id, payload={})
    assert_problem_details(r, 400)


def test_rules_patch_not_found(authed, api_up):
    r = authed.rules_patch(str(uuid.uuid4()), payload={"name": "New Name"})
    assert_problem_details(r, 404)


def test_rules_patch_invalid_name_empty(authed, api_up):
    body = _list_rules(authed, page=1, pageSize=1)
    if not body["items"]:
        pytest.skip("No rules seeded; cannot test PATCH.")
    rule_id = body["items"][0]["id"]

    # Domain best practice: empty/whitespace name => 400, not 500.
    r = authed.rules_patch(rule_id, payload={"name": "   "})
    assert_problem_details(r, 400)


def test_rules_patch_invalid_parameters_daily_limit(authed, api_up):
    """
    DailyLimit validator requires: limitBaseAmount > 0
    Payload key must be camelCase: parameters
    """
    body = _list_rules(authed, page=1, pageSize=50)
    items = body["items"]
    if not items:
        pytest.skip("No rules seeded; cannot test parameters validation.")

    # Assumed enum numeric order: DailyLimit = 0 (adjust if your enum differs)
    rule = _pick_rule(items, rule_type_name="DailyLimit", rule_type_numeric=0)
    if not rule:
        pytest.skip("No DailyLimit rule seeded; cannot test DailyLimit validation.")

    rule_id = rule["id"]

    # Missing required field
    r = authed.rules_patch(rule_id, payload={"parameters": {}})
    assert_problem_details(r, 400)

    # Invalid (<= 0)
    r = authed.rules_patch(rule_id, payload={"parameters": {"limitBaseAmount": 0}})
    assert_problem_details(r, 400)


def test_rules_patch_invalid_parameters_banned_countries(authed, api_up):
    """
    BannedCountries validator requires: countries array of ISO alpha-2
    """
    body = _list_rules(authed, page=1, pageSize=50)
    items = body["items"]
    if not items:
        pytest.skip("No rules seeded; cannot test parameters validation.")

    # Assumed enum numeric order: BannedCountries = 1 (adjust if your enum differs)
    rule = _pick_rule(items, rule_type_name="BannedCountries", rule_type_numeric=1)
    if not rule:
        pytest.skip("No BannedCountries rule seeded; cannot test BannedCountries validation.")

    rule_id = rule["id"]

    # Missing array
    r = authed.rules_patch(rule_id, payload={"parameters": {}})
    assert_problem_details(r, 400)

    # Wrong element type
    r = authed.rules_patch(rule_id, payload={"parameters": {"countries": [123]}})
    assert_problem_details(r, 400)

    # Wrong length (should be 2)
    r = authed.rules_patch(rule_id, payload={"parameters": {"countries": ["BRA"]}})
    assert_problem_details(r, 400)


def test_rules_patch_valid_update_name_and_restore(authed, api_up):
    """
    Performs a safe, reversible update:
      - fetch one rule
      - patch name
      - verify
      - restore original name
    """
    body = _list_rules(authed, page=1, pageSize=1)
    if not body["items"]:
        pytest.skip("No rules seeded; cannot test PATCH.")
    rule = body["items"][0]
    rule_id = rule["id"]
    original_name = rule["name"]

    new_name = f"{original_name} (patched)"
    r = authed.rules_patch(rule_id, payload={"name": new_name})
    assert_status(r, 200)
    updated = r.json()
    assert updated.get("name") == new_name

    # Restore
    r = authed.rules_patch(rule_id, payload={"name": original_name})
    assert_status(r, 200)
    restored = r.json()
    assert restored.get("name") == original_name


def test_rules_patch_toggle_is_active_and_restore(authed, api_up):
    """
    Toggle isActive off and restore original.
    """
    body = _list_rules(authed, page=1, pageSize=1)
    if not body["items"]:
        pytest.skip("No rules seeded; cannot test PATCH.")
    rule = body["items"][0]
    rule_id = rule["id"]
    original = rule["isActive"]

    r = authed.rules_patch(rule_id, payload={"isActive": (not original)})
    assert_status(r, 200)
    updated = r.json()
    assert updated.get("isActive") == (not original)

    # Restore
    r = authed.rules_patch(rule_id, payload={"isActive": original})
    assert_status(r, 200)
    restored = r.json()
    assert restored.get("isActive") == original
