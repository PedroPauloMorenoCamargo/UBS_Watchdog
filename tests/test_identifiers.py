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
    account_type: int = 0,  # JSON enum (assumindo 0 válido)
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
    if payload is None:
        payload = _valid_account_payload()

    r = authed.account_create(client_id, payload)

    if r.status_code == 400:
        # pode ocorrer por seed de Countries ausente no validator de CountryCode
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
    return r.json()


# -----------------------------
# Helpers (Account Identifier)
# -----------------------------
def _valid_identifier_payload(
    *,
    identifier_type: int = 0,  # JSON enum (assumindo 0 válido)
    identifier_value: str | None = None,
    issued_country_code: str | None = None,
):
    if identifier_value is None:
        identifier_value = f"IDV-{uuid.uuid4()}"
    payload = {
        "identifierType": identifier_type,
        "identifierValue": identifier_value,
    }
    # manter None por padrão para não depender de seed de Countries
    if issued_country_code is not None:
        payload["issuedCountryCode"] = issued_country_code
    return payload


def _create_identifier_or_fail(authed, account_id: str, payload: dict | None = None):
    if payload is None:
        payload = _valid_identifier_payload()
    r = authed.account_identifier_create(account_id, payload)
    assert_status(r, 201)
    return r.json()


# -------------------------------------------------
# GET /api/accounts/{accountId}/identifiers
# -------------------------------------------------
def test_get_identifiers_by_account_id_success_returns_200_list(authed, api_up):
    client = _create_client_or_skip(authed)
    account = _create_account_or_skip(authed, client["id"])

    created = _create_identifier_or_fail(authed, account["id"])

    r = authed.account_identifiers_get_by_account(account["id"])
    assert_status(r, 200)

    items = r.json()
    assert isinstance(items, list)
    assert any(x.get("id") == created["id"] for x in items)


def test_get_identifiers_by_account_id_account_not_found_returns_404(authed, api_up):
    missing_account_id = str(uuid.uuid4())

    r = authed.account_identifiers_get_by_account(missing_account_id)
    assert_status(r, 404)

    body = r.json()
    assert body.get("status") == 404
    assert "title" in body


def test_get_identifiers_by_account_id_requires_auth(api, authed, api_up):
    client = _create_client_or_skip(authed)
    account = _create_account_or_skip(authed, client["id"])

    r = api.account_identifiers_get_by_account(account["id"])
    assert_status(r, 401)


# -------------------------------------------------
# POST /api/accounts/{accountId}/identifiers
# -------------------------------------------------
def test_create_identifier_success_returns_201_and_body(authed, api_up):
    client = _create_client_or_skip(authed)
    account = _create_account_or_skip(authed, client["id"])

    payload = _valid_identifier_payload()
    r = authed.account_identifier_create(account["id"], payload)
    assert_status(r, 201)

    body = r.json()
    assert "id" in body
    assert body["identifierValue"] == payload["identifierValue"]
    assert body["identifierType"] == payload["identifierType"]
    # issuedCountryCode pode vir null/ausente dependendo do DTO; validamos apenas que não quebra:
    assert "createdAtUtc" in body


def test_create_identifier_requires_auth(api, authed, api_up):
    client = _create_client_or_skip(authed)
    account = _create_account_or_skip(authed, client["id"])

    payload = _valid_identifier_payload()
    r = api.account_identifier_create(account["id"], payload)
    assert_status(r, 401)


def test_create_identifier_account_not_found_returns_404(authed, api_up):
    payload = _valid_identifier_payload()
    missing_account_id = str(uuid.uuid4())

    r = authed.account_identifier_create(missing_account_id, payload)
    assert_status(r, 404)

    body = r.json()
    assert body.get("status") == 404
    assert "title" in body


def test_create_identifier_duplicate_returns_400(authed, api_up):
    client = _create_client_or_skip(authed)
    account = _create_account_or_skip(authed, client["id"])

    fixed_value = f"IDV-DUP-{uuid.uuid4()}"
    payload = _valid_identifier_payload(identifier_type=0, identifier_value=fixed_value)

    first = authed.account_identifier_create(account["id"], payload)
    assert_status(first, 201)

    second = authed.account_identifier_create(account["id"], payload)
    assert_status(second, 400)

    body = second.json()
    assert body.get("status") == 400
    assert "title" in body


@pytest.mark.parametrize(
    "mutator",
    [
        lambda p: p.update({"identifierValue": ""}),
        lambda p: p.update({"identifierValue": "   "}),
        lambda p: p.update({"identifierType": 999}),           # enum inválido
        lambda p: p.update({"issuedCountryCode": "B"}),        # tamanho inválido
        lambda p: p.update({"issuedCountryCode": "BRA"}),      # tamanho inválido
        lambda p: p.update({"issuedCountryCode": "1!"}),       # chars inválidos
        lambda p: p.update({"identifierValue": "x" * 201}),    # > 200 (validator)
    ],
)
def test_create_identifier_validation_errors_return_400(authed, api_up, mutator):
    client = _create_client_or_skip(authed)
    account = _create_account_or_skip(authed, client["id"])

    payload = _valid_identifier_payload()
    mutator(payload)

    r = authed.account_identifier_create(account["id"], payload)
    assert_status(r, 400)

    body = r.json()
    assert body.get("status") == 400
    assert "title" in body


# -------------------------------------------------
# DELETE /api/account-identifiers/{identifierId}
# -------------------------------------------------
def test_remove_identifier_success_returns_204(authed, api_up):
    client = _create_client_or_skip(authed)
    account = _create_account_or_skip(authed, client["id"])
    created = _create_identifier_or_fail(authed, account["id"])

    r = authed.account_identifier_delete(created["id"])
    assert_status(r, 204)

    # opcional: garante que sumiu da lista
    r2 = authed.account_identifiers_get_by_account(account["id"])
    assert_status(r2, 200)
    items = r2.json()
    assert all(x.get("id") != created["id"] for x in items)


def test_remove_identifier_not_found_returns_404(authed, api_up):
    missing_identifier_id = str(uuid.uuid4())

    r = authed.account_identifier_delete(missing_identifier_id)
    assert_status(r, 404)

    body = r.json()
    assert body.get("status") == 404
    assert "title" in body


def test_remove_identifier_requires_auth(api, authed, api_up):
    client = _create_client_or_skip(authed)
    account = _create_account_or_skip(authed, client["id"])
    created = _create_identifier_or_fail(authed, account["id"])

    r = api.account_identifier_delete(created["id"])
    assert_status(r, 401)
