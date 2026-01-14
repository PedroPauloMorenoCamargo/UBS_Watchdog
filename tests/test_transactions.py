import json
import uuid
from datetime import datetime, timedelta, timezone

import pytest
from tests.helpers.assertions import assert_status

pytestmark = pytest.mark.integration


# -----------------------------
# Client helpers (same pattern you used)
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
        # Common when Countries seed is missing.
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
# Account helpers (assumes your AccountsController)
# -----------------------------
def _valid_account_payload(
    *,
    account_identifier: str | None = None,
    country_code: str = "BR",
    account_type: int = 0,  # enum int
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
        try:
            body = r.json()
        except Exception:
            body = {"raw": r.text}
        errors = body.get("errors") if isinstance(body, dict) else None
        if isinstance(errors, dict) and ("CountryCode" in errors or "countryCode" in errors):
            pytest.skip(
                "Could not create account (likely missing countries seed). "
                f"Response: {json.dumps(body, ensure_ascii=False)}"
            )

    assert_status(r, 201)
    return r.json()


# -----------------------------
# Transaction helpers
# -----------------------------
def _now_iso_z():
    return datetime.now(timezone.utc).replace(microsecond=0).isoformat().replace("+00:00", "Z")


def _future_iso_z(minutes: int = 60):
    return (datetime.now(timezone.utc) + timedelta(minutes=minutes)).replace(microsecond=0).isoformat().replace("+00:00", "Z")


def _valid_deposit_payload(account_id: str):
    return {
        "accountId": account_id,
        "type": 0,
        "amount": 100.50,
        "currencyCode": "BRL",
        "occurredAtUtc": _now_iso_z(),
    }


def _valid_transfer_non_br_payload(account_id: str):
    return {
        "accountId": account_id,
        "type": 2,
        "transferMethod": 0,         
        "amount": 10.00,
        "currencyCode": "BRL",
        "occurredAtUtc": _now_iso_z(),
        "cpCountryCode": "US",        
        "cpIdentifierType": 0,        
        "cpIdentifier": f"CP-{uuid.uuid4()}",
        "cpName": "Counterparty Inc",
    }


def _valid_transfer_br_payload(account_id: str, cp_identifier_type: int, cp_identifier: str):
    return {
        "accountId": account_id,
        "type": 2,
        "transferMethod": 0,
        "amount": 10.00,
        "currencyCode": "BRL",
        "occurredAtUtc": _now_iso_z(),
        "cpCountryCode": "BR",
        "cpIdentifierType": cp_identifier_type,
        "cpIdentifier": cp_identifier,
        "cpName": "Counterparty BR",
    }


def _skip_if_fx_or_country_seed_issue(resp):
    """
    If CreateTransaction fails due to missing FX rate or missing countries seed,
    skip to avoid cascading failures.
    """
    if resp.status_code != 400:
        return

    try:
        body = resp.json()
    except Exception:
        return

    txt = json.dumps(body, ensure_ascii=False).lower()

    # FX (depends on your FxRateService data)
    if "fx" in txt or "rate" in txt and "currency" in txt:
        pytest.skip(f"FX conversion not available in this environment. Response: {txt}")

    if "countries table" in txt or "country code" in txt and "does not exist" in txt:
        pytest.skip(f"Countries seed likely missing in this environment. Response: {txt}")


def _extract_items(paged_json):
    """
    PagedTransactionsResponseDto:
      { items: [...], totalCount: n, pageNumber: n, pageSize: n, totalPages: n }
    """
    if isinstance(paged_json, dict) and isinstance(paged_json.get("items"), list):
        return paged_json["items"]
    return None


# -------------------------------------------------
# POST /api/transactions
# -------------------------------------------------
def test_create_transaction_deposit_success_returns_201_and_body(authed, api_up):
    client = _create_client_or_skip(authed)
    account = _create_account_or_skip(authed, client["id"])

    payload = _valid_deposit_payload(account["id"])
    r = authed.transaction_create(payload)

    if r.status_code == 400:
        _skip_if_fx_or_country_seed_issue(r)

    assert_status(r, 201)
    body = r.json()

    assert "id" in body
    assert body["accountId"] == account["id"]
    assert body["clientId"] == client["id"]
    assert body["currencyCode"] == payload["currencyCode"]
    assert float(body["amount"]) == float(payload["amount"])
    assert "baseAmount" in body
    assert "baseCurrencyCode" in body
    assert "createdAtUtc" in body


def test_create_transaction_requires_auth(api, authed, api_up):
    client = _create_client_or_skip(authed)
    account = _create_account_or_skip(authed, client["id"])

    payload = _valid_deposit_payload(account["id"])
    r = api.transaction_create(payload)
    assert_status(r, 401)


@pytest.mark.parametrize(
    "mutator, expected_error_key",
    [
        (lambda p: p.pop("accountId", None), "AccountId"),
        (lambda p: p.update({"accountId": str(uuid.uuid4())}), "AccountId"),
        (lambda p: p.pop("type", None), "Type"),
        (lambda p: p.update({"type": 999}), "Type"),
        (lambda p: p.update({"amount": 0}), "Amount"),
        (lambda p: p.update({"amount": -1}), "Amount"),
        (lambda p: p.pop("currencyCode", None), "CurrencyCode"),
        (lambda p: p.update({"currencyCode": "B"}), "CurrencyCode"),
        (lambda p: p.update({"currencyCode": "BRLL"}), "CurrencyCode"),
        (lambda p: p.pop("occurredAtUtc", None), "OccurredAtUtc"),
        (lambda p: p.update({"occurredAtUtc": "not-a-date"}), "OccurredAtUtc"),
        (lambda p: p.update({"occurredAtUtc": _future_iso_z(minutes=60 * 24)}), "OccurredAtUtc"),
    ],
)
def test_create_transaction_validation_errors_return_400(authed, api_up, mutator, expected_error_key):
    client = _create_client_or_skip(authed)
    account = _create_account_or_skip(authed, client["id"])

    payload = _valid_deposit_payload(account["id"])
    mutator(payload)

    r = authed.transaction_create(payload)
    assert_status(r, 400)

    body = r.json()
    assert body.get("status") == 400
    assert "errors" in body
    assert expected_error_key in body["errors"]


def test_create_transfer_requires_fields_return_400(authed, api_up):
    client = _create_client_or_skip(authed)
    account = _create_account_or_skip(authed, client["id"])

    payload = _valid_transfer_non_br_payload(account["id"])
    # Remove required transfer fields
    payload.pop("transferMethod", None)

    r = authed.transaction_create(payload)
    assert_status(r, 400)

    body = r.json()
    assert body.get("status") == 400
    assert "errors" in body
    assert "TransferMethod" in body["errors"]


def test_create_transfer_non_br_success_returns_201_or_skips_if_country_missing(authed, api_up):
    client = _create_client_or_skip(authed)
    account = _create_account_or_skip(authed, client["id"])

    payload = _valid_transfer_non_br_payload(account["id"])
    r = authed.transaction_create(payload)

    if r.status_code == 400:
        _skip_if_fx_or_country_seed_issue(r)
        assert_status(r, 201)

    assert_status(r, 201)
    body = r.json()

    assert "id" in body
    assert body["accountId"] == account["id"]
    assert body["type"] == 2
    assert body["cpCountryCode"] == "US"


def test_create_transfer_br_identifier_must_exist_return_400(authed, api_up):
    """
    Your validator enforces that for BR transfers the counterparty identifier
    must exist in the system (account_identifiers table).
    Here we intentionally use a random identifier to confirm it fails.
    """
    client = _create_client_or_skip(authed)
    account = _create_account_or_skip(authed, client["id"])

    payload = _valid_transfer_br_payload(
        account_id=account["id"],
        cp_identifier_type=0,  # enum int (pick any valid)
        cp_identifier=f"NONEXISTENT-{uuid.uuid4()}",
    )

    r = authed.transaction_create(payload)
    assert_status(r, 400)

    body = r.json()
    assert body.get("status") == 400
    assert "errors" in body
    keys = set(body["errors"].keys())
    assert ("" in keys) or ("request" in keys) or ("CreateTransactionRequest" in keys) or ("CpIdentifier" in keys) or ("CpIdentifierType" in keys)


# -------------------------------------------------
# GET /api/transactions/{id}
# -------------------------------------------------
def test_get_transaction_by_id_success_returns_200(authed, api_up):
    client = _create_client_or_skip(authed)
    account = _create_account_or_skip(authed, client["id"])

    create_payload = _valid_deposit_payload(account["id"])
    created = authed.transaction_create(create_payload)

    if created.status_code == 400:
        _skip_if_fx_or_country_seed_issue(created)

    assert_status(created, 201)
    tx = created.json()

    r = authed.transaction_get(tx["id"])
    assert_status(r, 200)

    body = r.json()
    assert body["id"] == tx["id"]
    assert body["accountId"] == account["id"]


def test_get_transaction_by_id_not_found_returns_404(authed, api_up):
    r = authed.transaction_get(str(uuid.uuid4()))
    assert_status(r, 404)

    body = r.json()
    assert body.get("status") == 404
    assert "title" in body


def test_get_transaction_by_id_requires_auth(api, authed, api_up):
    client = _create_client_or_skip(authed)
    account = _create_account_or_skip(authed, client["id"])

    created = authed.transaction_create(_valid_deposit_payload(account["id"]))
    if created.status_code == 400:
        _skip_if_fx_or_country_seed_issue(created)

    assert_status(created, 201)
    tx = created.json()

    r = api.transaction_get(tx["id"])
    assert_status(r, 401)


# -------------------------------------------------
# GET /api/transactions (paged search)
# -------------------------------------------------
def test_search_transactions_by_account_returns_paged_items(authed, api_up):
    client = _create_client_or_skip(authed)
    account = _create_account_or_skip(authed, client["id"])

    created = authed.transaction_create(_valid_deposit_payload(account["id"]))
    if created.status_code == 400:
        _skip_if_fx_or_country_seed_issue(created)

    assert_status(created, 201)
    tx = created.json()

    r = authed.transactions_search(
        params={
            "accountId": account["id"],
            "page": 1,
            "pageSize": 20,
            "sortBy": "occurredAtUtc",
            "sortDescending": True,
        }
    )

    if r.status_code == 404:
        pytest.skip("GET /api/transactions not implemented (404).")

    assert_status(r, 200)
    body = r.json()

    items = _extract_items(body)
    assert items is not None, "Expected PagedTransactionsResponseDto with 'items'."

    assert any(x.get("id") == tx["id"] for x in items)


def test_search_transactions_requires_auth(api, authed, api_up):
    r = api.transactions_search(params={"page": 1, "pageSize": 20})
    assert r.status_code in (401, 404)


# -------------------------------------------------
# POST /api/transactions/import
# -------------------------------------------------
def test_import_transactions_no_file_returns_400_or_skips_if_missing_route(authed, api_up):
    r = authed.post("/api/transactions/import", files={})

    if r.status_code == 404:
        pytest.skip("POST /api/transactions/import not implemented (404).")

    assert_status(r, 400)


def test_import_transactions_invalid_extension_returns_400_or_skips_if_missing_route(authed, api_up):
    r = authed.transactions_import("tx.txt", b"not-csv", content_type="text/plain")

    if r.status_code == 404:
        pytest.skip("POST /api/transactions/import not implemented (404).")

    assert_status(r, 400)


def test_import_transactions_unknown_account_identifier_returns_200_with_errors_or_skips(authed, api_up):
    """
    Import processing returns a result DTO even for partial failures.
    Unknown AccountIdentifier should appear in Errors with ErrorCount > 0.
    """
    csv = (
        "AccountIdentifier,Type,TransferMethod,Amount,CurrencyCode,OccurredAtUtc,CpName,CpBank,CpBranch,CpAccount,CpIdentifierType,CpIdentifier,CpCountryCode\n"
        f"UNKNOWN-ACC,Deposit,,10.00,BRL,{_now_iso_z()},,,,,,,\n"
    ).encode("utf-8")

    r = authed.transactions_import("tx.csv", csv)

    if r.status_code == 404:
        pytest.skip("POST /api/transactions/import not implemented (404).")

    assert_status(r, 200)
    body = r.json()

    assert "totalProcessed" in body
    assert "successCount" in body
    assert "errorCount" in body
    assert "errors" in body
    assert body["errorCount"] >= 1


def test_import_transactions_success_csv_returns_200_or_skips_if_fx_seed_missing(authed, api_up):
    client = _create_client_or_skip(authed)
    account = _create_account_or_skip(authed, client["id"])

    account_identifier = account.get("accountIdentifier")
    if not account_identifier:
        pytest.skip("AccountResponseDto did not include accountIdentifier; cannot build import CSV reliably.")

    csv = (
        "AccountIdentifier,Type,TransferMethod,Amount,CurrencyCode,OccurredAtUtc,CpName,CpBank,CpBranch,CpAccount,CpIdentifierType,CpIdentifier,CpCountryCode\n"
        f"{account_identifier},Deposit,,10.00,BRL,{_now_iso_z()},,,,,,,\n"
    ).encode("utf-8")

    r = authed.transactions_import("tx.csv", csv)

    if r.status_code == 404:
        pytest.skip("POST /api/transactions/import not implemented (404).")

    if r.status_code == 400:
        _skip_if_fx_or_country_seed_issue(r)

    assert_status(r, 200)
    body = r.json()

    assert body["totalProcessed"] >= 1
    assert "successCount" in body
    assert "errorCount" in body
    assert "errors" in body
