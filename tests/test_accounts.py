import json
import uuid

import pytest
from tests.helpers.assertions import assert_status

pytestmark = pytest.mark.integration


# -----------------------------
# Helpers (Client)
# -----------------------------
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
    Cria um client e retorna o JSON.
    Se o ambiente não tiver o seed de Countries (ex.: BR), o validator pode retornar 400.
    Nesse caso, skip para evitar falhas em cascata.
    """
    payload = _valid_client_payload()
    r = authed.client_create(payload)

    if r.status_code == 400:
        try:
            body = r.json()
        except Exception:
            body = {"raw": r.text}
        pytest.skip(
            "Could not create client (likely missing countries seed). "
            f"Response: {json.dumps(body, ensure_ascii=False)}"
        )

    assert_status(r, 201)
    return r.json()


# -----------------------------
# Helpers (Account)
# -----------------------------
def _valid_account_payload(
    *,
    account_identifier: str | None = None,
    country_code: str = "BR",
    account_type: int = 0,  # assume 0 = Checking (default JSON enums are numeric in your API)
    currency_code: str = "BRL",
):
    if account_identifier is None:
        account_identifier = f"ACC-{uuid.uuid4()}"
    return {
        "accountIdentifier": account_identifier,
        "countryCode": country_code,
        "accountType": account_type,
        "currencyCode": currency_code,
    }


def _create_account_or_skip(authed, client_id: str, payload: dict | None = None):
    """
    Cria uma account e retorna o JSON.
    Se retornar 400 por causa de validator/seed do Countries (CountryCode),
    fazemos skip para não quebrar os testes de 'happy path'.
    """
    if payload is None:
        payload = _valid_account_payload()

    r = authed.account_create(client_id, payload)

    if r.status_code == 400:
        # Pode ser Countries seed faltando (validator de CountryCode).
        try:
            body = r.json()
        except Exception:
            body = {"raw": r.text}

        # Se parece que o problema é country seed/validation, skip.
        # Caso contrário, deixe falhar nos testes que esperam 201.
        errors = body.get("errors") if isinstance(body, dict) else None
        if isinstance(errors, dict) and ("CountryCode" in errors or "countryCode" in errors):
            pytest.skip(
                "Could not create account (likely missing countries seed for account validator). "
                f"Response: {json.dumps(body, ensure_ascii=False)}"
            )

    assert_status(r, 201)
    return r.json()


def _csv_bytes_for_import(*, account_identifier: str, country_code: str = "BR"):
    # AccountImportRow headers:
    # AccountIdentifier, CountryCode, AccountType, CurrencyCode
    # AccountType here is parsed from STRING: Checking/Savings/Investment/Other
    csv_text = (
        "AccountIdentifier,CountryCode,AccountType,CurrencyCode\n"
        f"{account_identifier},{country_code},Checking,BRL\n"
    )
    return csv_text.encode("utf-8")


# -------------------------------------------------
# CreateAccount
# -------------------------------------------------
def test_create_account_success_returns_201_and_body(authed, api_up):
    client = _create_client_or_skip(authed)
    payload = _valid_account_payload()

    r = authed.account_create(client["id"], payload)
    # Se o env não tiver Countries seed para validator do account, o helper acima não foi usado.
    # Aqui queremos realmente o happy path.
    if r.status_code == 400:
        try:
            body = r.json()
        except Exception:
            body = {"raw": r.text}
        errors = body.get("errors") if isinstance(body, dict) else None
        if isinstance(errors, dict) and ("CountryCode" in errors or "countryCode" in errors):
            pytest.skip(
                "Could not create account (likely missing countries seed for account validator). "
                f"Response: {json.dumps(body, ensure_ascii=False)}"
            )

    assert_status(r, 201)
    body = r.json()

    assert "id" in body
    assert body["clientId"] == client["id"]
    assert body["accountIdentifier"] == payload["accountIdentifier"]
    assert body["countryCode"] == payload["countryCode"]
    assert body["currencyCode"] == payload["currencyCode"]
    assert "createdAtUtc" in body
    assert "updatedAtUtc" in body


def test_create_account_requires_auth(api, authed, api_up):
    client = _create_client_or_skip(authed)
    payload = _valid_account_payload()

    r = api.account_create(client["id"], payload)
    assert_status(r, 401)


def test_create_account_client_not_found_returns_404(authed, api_up):
    payload = _valid_account_payload()
    missing_client_id = str(uuid.uuid4())

    r = authed.account_create(missing_client_id, payload)
    assert_status(r, 404)

    body = r.json()
    assert body.get("status") == 404
    assert "title" in body


def test_create_account_duplicate_identifier_returns_400(authed, api_up):
    client = _create_client_or_skip(authed)

    fixed_identifier = f"ACC-DUP-{uuid.uuid4()}"
    payload = _valid_account_payload(account_identifier=fixed_identifier)

    # First create should succeed (or skip if env invalid)
    created = _create_account_or_skip(authed, client["id"], payload)
    assert created["accountIdentifier"] == fixed_identifier

    # Second create should be 400 (either model validation or controller ProblemDetails)
    r2 = authed.account_create(client["id"], payload)
    assert_status(r2, 400)

    body = r2.json()
    assert body.get("status") == 400
    assert "title" in body


@pytest.mark.parametrize(
    "mutator",
    [
        lambda p: p.update({"accountIdentifier": ""}),
        lambda p: p.update({"accountIdentifier": "   "}),
        lambda p: p.update({"countryCode": ""}),
        lambda p: p.update({"countryCode": "B"}),
        lambda p: p.update({"countryCode": "BRA"}),
        lambda p: p.update({"countryCode": "1!"}),
        lambda p: p.update({"currencyCode": ""}),
        lambda p: p.update({"currencyCode": "BR"}),     # too short
        lambda p: p.update({"currencyCode": "BRLL"}),   # too long
        lambda p: p.update({"currencyCode": "12$"}),    # invalid chars
        lambda p: p.update({"accountType": 999}),       # invalid enum
    ],
)
def test_create_account_validation_errors_return_400(authed, api_up, mutator):
    client = _create_client_or_skip(authed)
    payload = _valid_account_payload()
    mutator(payload)

    r = authed.account_create(client["id"], payload)
    assert_status(r, 400)

    body = r.json()
    assert body.get("status") == 400
    assert "title" in body


# -------------------------------------------------
# GetAccountsByClientId
# -------------------------------------------------
def test_get_accounts_by_client_id_success_returns_200_list(authed, api_up):
    client = _create_client_or_skip(authed)
    created = _create_account_or_skip(authed, client["id"])

    r = authed.accounts_get_by_client(client["id"])
    assert_status(r, 200)

    items = r.json()
    assert isinstance(items, list)
    assert any(a.get("id") == created["id"] for a in items)


def test_get_accounts_by_client_id_client_not_found_returns_404(authed, api_up):
    missing_client_id = str(uuid.uuid4())

    r = authed.accounts_get_by_client(missing_client_id)
    assert_status(r, 404)

    body = r.json()
    assert body.get("status") == 404
    assert "title" in body


def test_get_accounts_by_client_id_requires_auth(api, authed, api_up):
    client = _create_client_or_skip(authed)

    r = api.accounts_get_by_client(client["id"])
    assert_status(r, 401)


# -------------------------------------------------
# GetAccountById
# -------------------------------------------------
def test_get_account_by_id_success_returns_200_detail(authed, api_up):
    client = _create_client_or_skip(authed)
    created = _create_account_or_skip(authed, client["id"])

    r = authed.account_get(created["id"])
    assert_status(r, 200)

    body = r.json()
    assert body["id"] == created["id"]
    assert body["clientId"] == client["id"]
    assert "identifiers" in body
    assert isinstance(body["identifiers"], list)


def test_get_account_by_id_not_found_returns_404(authed, api_up):
    missing_account_id = str(uuid.uuid4())

    r = authed.account_get(missing_account_id)
    assert_status(r, 404)

    body = r.json()
    assert body.get("status") == 404
    assert "title" in body


def test_get_account_by_id_requires_auth(api, authed, api_up):
    client = _create_client_or_skip(authed)
    created = _create_account_or_skip(authed, client["id"])

    r = api.account_get(created["id"])
    assert_status(r, 401)


# -------------------------------------------------
# ImportAccounts
# -------------------------------------------------
def test_import_accounts_no_file_returns_400(authed, api_up):
    client = _create_client_or_skip(authed)

    # No multipart "file" field -> should hit controller "No file provided"
    r = authed.post(f"/api/clients/{client['id']}/accounts/import")
    assert_status(r, 400)

    body = r.json()
    assert body.get("status") == 400
    assert "title" in body


def test_import_accounts_invalid_extension_returns_400(authed, api_up):
    client = _create_client_or_skip(authed)

    file_bytes = _csv_bytes_for_import(account_identifier=f"ACC-IMP-{uuid.uuid4()}")
    r = authed.accounts_import(client["id"], "accounts.txt", file_bytes)
    assert_status(r, 400)

    body = r.json()
    assert body.get("status") == 400
    assert "title" in body


def test_import_accounts_client_not_found_returns_404(authed, api_up):
    missing_client_id = str(uuid.uuid4())

    file_bytes = _csv_bytes_for_import(account_identifier=f"ACC-IMP-{uuid.uuid4()}")
    r = authed.accounts_import(missing_client_id, "accounts.csv", file_bytes)
    assert_status(r, 404)

    body = r.json()
    assert body.get("status") == 404
    assert "title" in body


def test_import_accounts_requires_auth(api, authed, api_up):
    client = _create_client_or_skip(authed)

    file_bytes = _csv_bytes_for_import(account_identifier=f"ACC-IMP-{uuid.uuid4()}")
    r = api.accounts_import(client["id"], "accounts.csv", file_bytes)
    assert_status(r, 401)


def test_import_accounts_success_csv_returns_200(authed, api_up):
    client = _create_client_or_skip(authed)

    account_identifier = f"ACC-IMP-{uuid.uuid4()}"
    file_bytes = _csv_bytes_for_import(account_identifier=account_identifier)

    r = authed.accounts_import(client["id"], "accounts.csv", file_bytes)
    assert_status(r, 200)

    body = r.json()
    # AccountImportResultDto: totalProcessed, successCount, errorCount, errors
    assert "totalProcessed" in body
    assert "successCount" in body
    assert "errorCount" in body
    assert "errors" in body
    assert isinstance(body["errors"], list)
    assert body["totalProcessed"] >= 1
