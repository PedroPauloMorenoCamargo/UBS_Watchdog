import json
import uuid
from datetime import datetime, timezone

import pytest
from tests.helpers.assertions import assert_status

pytestmark = pytest.mark.integration


# -----------------------------
# Small utilities
# -----------------------------
def _now_iso_z():
    return datetime.now(timezone.utc).replace(microsecond=0).isoformat().replace("+00:00", "Z")


def _paged_items(payload: dict) -> list:
    # Be defensive across possible casing/DTO naming
    if not isinstance(payload, dict):
        return []
    for key in ("items", "Items", "data", "Data"):
        v = payload.get(key)
        if isinstance(v, list):
            return v
    return []


def _get_rule_by_code_or_skip(authed, code: str) -> dict:
    """
    Fetch rules and find by stable Code.
    If rules endpoint is not available or rule is missing, skip (seed config differs).
    """
    r = authed.rules_search(params={"page": 1, "pageSize": 100})
    if r.status_code == 404:
        pytest.skip("Rules endpoint not available (GET /api/rules returned 404).")
    assert_status(r, 200)

    body = r.json()
    items = _paged_items(body)
    rule = next((x for x in items if x.get("code") == code), None)
    if rule is None:
        pytest.skip(f"Seeded rule not found by code='{code}'. Seeding config differs.")
    return rule


def _find_case_by_transaction_id(authed, tx_id: str) -> dict | None:
    r = authed.cases_search(params={"transactionId": tx_id, "page": 1, "pageSize": 20})
    assert_status(r, 200)

    body = r.json()
    items = _paged_items(body)
    if not items:
        return None
    return items[0]


# -----------------------------
# Client + Account helpers
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
    payload = _valid_client_payload(name=f"Client-{uuid.uuid4()}")
    r = authed.client_create(payload)

    if r.status_code == 400:
        # Common if countries seed missing for BR
        try:
            body = r.json()
        except Exception:
            body = {"raw": r.text}
        pytest.skip(f"Could not create client (likely missing countries seed). Response: {json.dumps(body, ensure_ascii=False)}")

    assert_status(r, 201)
    return r.json()


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


def _create_account_or_skip(authed, client_id: str):
    payload = _valid_account_payload()
    r = authed.account_create(client_id, payload)

    if r.status_code == 400:
        try:
            body = r.json()
        except Exception:
            body = {"raw": r.text}
        pytest.skip(f"Could not create account. Response: {json.dumps(body, ensure_ascii=False)}")

    assert_status(r, 201)
    return r.json()


# -----------------------------
# Transaction payload builders
# -----------------------------
def _deposit_payload(account_id: str, *, amount: float, currency: str = "USD"):
    return {
        "accountId": account_id,
        "type": 0,  # Deposit
        "amount": amount,
        "currencyCode": currency,
        "occurredAtUtc": _now_iso_z(),
    }


def _transfer_payload(
    account_id: str,
    *,
    amount: float,
    currency: str = "USD",
    cp_country: str,
    cp_identifier_type: int = 0,
    cp_identifier: str | None = None,
    transfer_method: int = 0,  # PIX
    cp_name: str = "Counterparty",
):
    if cp_identifier is None:
        cp_identifier = f"CP-{uuid.uuid4()}"
    return {
        "accountId": account_id,
        "type": 2,  # Transfer
        "transferMethod": transfer_method,
        "amount": amount,
        "currencyCode": currency,
        "occurredAtUtc": _now_iso_z(),
        "cpCountryCode": cp_country,
        "cpIdentifierType": cp_identifier_type,
        "cpIdentifier": cp_identifier,
        "cpName": cp_name,
    }


# ============================================================
# Tests: Seeded rules -> create transactions -> verify cases
# ============================================================

def test_rule_1_daily_limit_default_creates_case_and_finding(authed, api_up):
    """
    daily_limit_default:
      RuleType = DailyLimit
      Parameters: limitBaseAmount = 10000
      Scope: PerClient
      Severity: Medium

    We create a single very large deposit in USD so baseAmount will exceed the limit regardless of base currency.
    Then verify a case exists for this transaction and includes finding with ruleCode=daily_limit_default.
    """
    rule = _get_rule_by_code_or_skip(authed, "daily_limit_default")
    expected_severity = rule.get("severity")

    client = _create_client_or_skip(authed)
    account = _create_account_or_skip(authed, client["id"])

    tx_payload = _deposit_payload(account["id"], amount=25000.00, currency="USD")
    tx_resp = authed.transaction_create(tx_payload)

    if tx_resp.status_code == 400:
        # Most common: FX/currency setup missing in environment
        try:
            body = tx_resp.json()
        except Exception:
            body = {"raw": tx_resp.text}
        pytest.skip(f"Could not create transaction (FX/currency config may differ). Response: {json.dumps(body, ensure_ascii=False)}")

    assert_status(tx_resp, 201)
    tx = tx_resp.json()

    case = _find_case_by_transaction_id(authed, tx["id"])
    assert case is not None, "Expected a case to be created for daily limit violation."

    assert case["transactionId"] == tx["id"]
    assert case["clientId"] == client["id"]
    if expected_severity is not None:
        assert case["severity"] == expected_severity

    # Validate findings include rule code
    findings_resp = authed.case_findings(case["id"])
    assert_status(findings_resp, 200)
    findings = findings_resp.json()

    assert isinstance(findings, list)
    assert any(f.get("ruleCode") == "daily_limit_default" for f in findings)


def test_rule_2_banned_countries_default_creates_case_and_finding(authed, api_up):
    """
    banned_countries_default:
      RuleType = BannedCountries
      Parameters: countries = [IR, KP, SY]
      Severity: High

    Create a transfer with cpCountryCode='IR' and confirm a case is created with finding ruleCode=banned_countries_default.
    If 'IR' is not seeded in countries table (validator rejects), skip.
    """
    rule = _get_rule_by_code_or_skip(authed, "banned_countries_default")
    expected_severity = rule.get("severity")

    client = _create_client_or_skip(authed)
    account = _create_account_or_skip(authed, client["id"])

    tx_payload = _transfer_payload(
        account["id"],
        amount=10.00,
        currency="USD",
        cp_country="IR",
    )
    tx_resp = authed.transaction_create(tx_payload)

    if tx_resp.status_code == 400:
        # If Countries table doesn't include IR, validator fails. Skip (environment differs).
        try:
            body = tx_resp.json()
        except Exception:
            body = {"raw": tx_resp.text}
        msg = json.dumps(body, ensure_ascii=False).lower()
        if "invalid country code" in msg or "countries table" in msg or "country code" in msg:
            pytest.skip(f"Country 'IR' not present in countries table in this environment. Response: {json.dumps(body, ensure_ascii=False)}")
        pytest.skip(f"Could not create transfer transaction. Response: {json.dumps(body, ensure_ascii=False)}")

    assert_status(tx_resp, 201)
    tx = tx_resp.json()

    case = _find_case_by_transaction_id(authed, tx["id"])
    assert case is not None, "Expected a case to be created for banned country violation."
    if expected_severity is not None:
        assert case["severity"] == expected_severity

    findings_resp = authed.case_findings(case["id"])
    assert_status(findings_resp, 200)
    findings = findings_resp.json()

    assert any(f.get("ruleCode") == "banned_countries_default" for f in findings)


def test_rule_3_structuring_default_creates_case_on_5th_transfer_only(authed, api_up):
    """
    structuring_default:
      RuleType = Structuring
      Parameters: n=5, xBaseAmount=2000
      Scope: PerClient
      Severity: High

    Create 4 transfers under threshold => no cases.
    Create 5th transfer under threshold => case created for that 5th transaction with finding structuring_default.

    Uses cpCountryCode='US' (must exist in countries table; if not, skip).
    Uses amount=100 USD so baseAmount should remain < 2000 for typical FX setups.
    """
    rule = _get_rule_by_code_or_skip(authed, "structuring_default")
    expected_severity = rule.get("severity")

    client = _create_client_or_skip(authed)
    account = _create_account_or_skip(authed, client["id"])

    # First 4: should NOT create cases
    tx_ids = []
    for _ in range(4):
        payload = _transfer_payload(
            account["id"],
            amount=100.00,
            currency="USD",
            cp_country="US",
        )
        resp = authed.transaction_create(payload)

        if resp.status_code == 400:
            try:
                body = resp.json()
            except Exception:
                body = {"raw": resp.text}
            msg = json.dumps(body, ensure_ascii=False).lower()
            if "invalid country code" in msg or "countries table" in msg or "country code" in msg:
                pytest.skip(f"Country 'US' not present in countries table in this environment. Response: {json.dumps(body, ensure_ascii=False)}")
            pytest.skip(f"Could not create transfer transaction (env differs). Response: {json.dumps(body, ensure_ascii=False)}")

        assert_status(resp, 201)
        tx = resp.json()
        tx_ids.append(tx["id"])

        case = _find_case_by_transaction_id(authed, tx["id"])
        assert case is None, "Did not expect structuring case before reaching N transfers."

    # 5th: should create case
    payload_5 = _transfer_payload(
        account["id"],
        amount=100.00,
        currency="USD",
        cp_country="US",
    )
    resp_5 = authed.transaction_create(payload_5)
    assert_status(resp_5, 201)
    tx5 = resp_5.json()

    case5 = _find_case_by_transaction_id(authed, tx5["id"])
    assert case5 is not None, "Expected structuring case to be created on the 5th transfer."
    if expected_severity is not None:
        assert case5["severity"] == expected_severity

    findings_resp = authed.case_findings(case5["id"])
    assert_status(findings_resp, 200)
    findings = findings_resp.json()

    assert any(f.get("ruleCode") == "structuring_default" for f in findings)


def test_rule_4_banned_accounts_default_inactive_no_violation_no_case_created(authed, api_up):
    """
    banned_accounts_default is seeded as IsActive=false and is not evaluated by TransactionComplianceChecker anyway.
    We validate the baseline expectation:
      - A normal transaction without violating daily_limit / banned_countries / structuring does NOT create a case.
    """
    _get_rule_by_code_or_skip(authed, "banned_accounts_default")

    client = _create_client_or_skip(authed)
    account = _create_account_or_skip(authed, client["id"])

    # Small deposit: should not exceed daily limit, and doesn't involve transfer-only rules.
    tx_payload = _deposit_payload(account["id"], amount=10.00, currency="USD")
    tx_resp = authed.transaction_create(tx_payload)

    if tx_resp.status_code == 400:
        try:
            body = tx_resp.json()
        except Exception:
            body = {"raw": tx_resp.text}
        pytest.skip(f"Could not create transaction (FX/currency config may differ). Response: {json.dumps(body, ensure_ascii=False)}")

    assert_status(tx_resp, 201)
    tx = tx_resp.json()

    case = _find_case_by_transaction_id(authed, tx["id"])
    assert case is None, "Did not expect a case for a non-violating transaction."


def test_case_detail_endpoint_returns_findings_and_entities(authed, api_up):
    """
    Sanity test for CasesController:
      - After a violation-generated case, GET /api/cases/{id} returns detail incl. findings.
    """
    _get_rule_by_code_or_skip(authed, "daily_limit_default")

    client = _create_client_or_skip(authed)
    account = _create_account_or_skip(authed, client["id"])

    tx_resp = authed.transaction_create(_deposit_payload(account["id"], amount=25000.00, currency="USD"))
    if tx_resp.status_code == 400:
        try:
            body = tx_resp.json()
        except Exception:
            body = {"raw": tx_resp.text}
        pytest.skip(f"Could not create transaction (FX/currency config may differ). Response: {json.dumps(body, ensure_ascii=False)}")

    assert_status(tx_resp, 201)
    tx = tx_resp.json()

    case = _find_case_by_transaction_id(authed, tx["id"])
    assert case is not None

    detail_resp = authed.case_get(case["id"])
    assert_status(detail_resp, 200)
    detail = detail_resp.json()

    assert detail["id"] == case["id"]
    assert detail["transactionId"] == tx["id"]
    assert "findings" in detail
    assert isinstance(detail["findings"], list)
    assert len(detail["findings"]) >= 1
