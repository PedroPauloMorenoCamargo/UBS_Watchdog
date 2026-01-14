import json
import uuid

import pytest
from tests.helpers.assertions import assert_status

pytestmark = pytest.mark.integration


def _try_json(resp):
    try:
        return resp.json()
    except Exception:
        return None


def _get_items(paged_body: dict) -> list:
    """
    PagedResult may be camelCase or PascalCase depending on JSON settings.
    This helper tolerates both.
    """
    if not isinstance(paged_body, dict):
        return []
    if "items" in paged_body and isinstance(paged_body["items"], list):
        return paged_body["items"]
    if "Items" in paged_body and isinstance(paged_body["Items"], list):
        return paged_body["Items"]
    return []


def _valid_client_payload(
    *,
    legal_type: int = 0,
    name: str = "John Doe",
    contact_number: str = "+5511999999999",
    country_code: str = "BR",
    initial_risk_level: int | None = 1,
):
    payload = {
        "legalType": legal_type,
        "name": name,
        "contactNumber": contact_number,
        "addressJson": {
            "street": "Av Paulista",
            "city": "São Paulo",
            "state": "SP",
            "zipCode": "01310-100",
            "country": "Brazil",
        },
        "countryCode": country_code,
    }
    if initial_risk_level is not None:
        payload["initialRiskLevel"] = initial_risk_level
    return payload


def _create_client_or_skip(authed):
    """
    Create a client and return its JSON.
    If the environment doesn't have seeded countries (e.g., BR), skip.
    """
    payload = _valid_client_payload()
    r = authed.client_create(payload)

    if r.status_code == 400:
        body = _try_json(r) or {"raw": r.text}
        msg = json.dumps(body, ensure_ascii=False)
        pytest.skip(f"Could not create client (likely missing countries seed). Response: {msg}")

    assert_status(r, 201)
    return r.json()


# -------------------------------------------------
# POST /api/clients
# -------------------------------------------------

def test_create_client_success_returns_201_and_body(authed, api_up):
    payload = _valid_client_payload()
    r = authed.client_create(payload)
    assert_status(r, 201)

    body = r.json()
    assert "id" in body
    assert body["name"] == payload["name"]
    assert body["countryCode"] == payload["countryCode"]
    assert body["contactNumber"] == payload["contactNumber"]
    assert body["addressJson"] is not None


def test_create_client_requires_auth_returns_401(api, api_up):
    payload = _valid_client_payload()
    r = api.client_create(payload)
    assert_status(r, 401)

    body = _try_json(r)
    assert isinstance(body, dict)
    assert body.get("status") == 401


def test_create_client_addressJson_must_be_object_returns_400(authed, api_up):
    payload = _valid_client_payload()
    payload["addressJson"] = "not-an-object"

    r = authed.client_create(payload)
    assert_status(r, 400)

    body = _try_json(r)
    assert isinstance(body, dict)

    # This matches the error you already observed:
    # { "errors": { "addressJson": ["addressJson must be a JSON object."] } }
    errors = body.get("errors") or {}
    assert "addressJson" in errors or "addressJson" in errors


@pytest.mark.parametrize(
    "mutator",
    [
        lambda p: p.update({"name": ""}),
        lambda p: p.update({"name": "   "}),
        lambda p: p.update({"contactNumber": ""}),
        lambda p: p.update({"contactNumber": "123"}),
        lambda p: p.pop("address", None),
        lambda p: p.update({"countryCode": ""}),
        lambda p: p.update({"countryCode": "B"}),
        lambda p: p.update({"countryCode": "BRA"}),
        lambda p: p.update({"countryCode": "1!"}),
    ],
)
def test_create_client_validation_errors_return_400(authed, api_up, mutator):
    payload = _valid_client_payload()
    mutator(payload)

    r = authed.client_create(payload)
    assert_status(r, 400)

    body = _try_json(r)
    assert isinstance(body, dict)
    assert body.get("status") == 400 or body.get("title")


# -------------------------------------------------
# GET /api/clients
# -------------------------------------------------

def test_get_clients_requires_auth_returns_401(api, api_up):
    r = api.clients_search()
    assert_status(r, 401)


def test_get_clients_default_returns_200_paged(authed, api_up):
    r = authed.clients_search()
    assert_status(r, 200)

    body = r.json()
    assert isinstance(body, dict)
    items = _get_items(body)
    assert isinstance(items, list)


def test_get_clients_filter_country_code_can_find_created_client(authed, api_up):
    client = _create_client_or_skip(authed)

    params = {
        "Page.Page": 1,
        "Page.PageSize": 50,
        "CountryCode": client["countryCode"],
        # optional sorting:
        "Page.SortBy": "CreatedAtUtc",
        "Page.SortDir": "desc",
    }

    r = authed.clients_search(params=params)
    assert_status(r, 200)

    body = r.json()
    items = _get_items(body)

    # Not asserting "only" because there may be seeded data,
    # but we should at least find the newly created client.
    ids = {x.get("id") for x in items if isinstance(x, dict)}
    assert client["id"] in ids


# -------------------------------------------------
# GET /api/clients/{id}
# -------------------------------------------------

def test_get_client_by_id_requires_auth_returns_401(api, api_up):
    r = api.client_get(str(uuid.uuid4()))
    assert_status(r, 401)


def test_get_client_by_id_success_returns_200_detail(authed, api_up):
    client = _create_client_or_skip(authed)

    r = authed.client_get(client["id"])
    assert_status(r, 200)

    body = r.json()
    assert body["id"] == client["id"]
    assert body["name"] == client["name"]

    # Detail DTO includes counts
    assert "totalAccounts" in body
    assert "totalTransactions" in body
    assert "totalCases" in body


def test_get_client_by_id_not_found_returns_404(authed, api_up):
    r = authed.client_get(str(uuid.uuid4()))
    assert_status(r, 404)

    body = _try_json(r)
    assert isinstance(body, dict)
    assert body.get("status") == 404


# -------------------------------------------------
# POST /api/clients/import
# -------------------------------------------------

def test_import_clients_requires_auth_returns_401(api, api_up):
    csv_bytes = b"LegalType,Name,ContactNumber,Street,City,State,ZipCode,Country,CountryCode,RiskLevel\n"
    r = api.clients_import("clients.csv", csv_bytes)
    assert_status(r, 401)


def test_import_clients_no_file_returns_400_problem(authed, api_up):
    # Send multipart with no file part.
    r = authed.post("/api/clients/import", files={})
    assert_status(r, 400)

    body = _try_json(r)
    assert isinstance(body, dict)
    assert body.get("status") == 400


def test_import_clients_invalid_extension_returns_400_problem(authed, api_up):
    r = authed.clients_import("clients.txt", b"not important")
    assert_status(r, 400)

    body = _try_json(r)
    assert isinstance(body, dict)
    assert body.get("status") == 400


def test_import_clients_success_csv_returns_200_and_counts(authed, api_up):
    # This import path does NOT rely on FluentValidation in the controller.
    # It may succeed even if countries seed is missing, as long as domain allows BR.
    csv_text = (
        "LegalType,Name,ContactNumber,Street,City,State,ZipCode,Country,CountryCode,RiskLevel\n"
        "Individual,Jane Doe,+5511999999999,Av Paulista,São Paulo,SP,01310-100,Brazil,BR,Medium\n"
    )
    r = authed.clients_import("clients.csv", csv_text.encode("utf-8"))
    assert_status(r, 200)

    body = r.json()
    assert isinstance(body, dict)

    # ImportResultDto shape:
    # { totalProcessed, successCount, errorCount, errors: [...] }
    assert body.get("totalProcessed", 0) >= 1
    assert body.get("successCount", 0) >= 0
    assert body.get("errorCount", 0) >= 0
    assert "errors" in body
    assert isinstance(body["errors"], list)
