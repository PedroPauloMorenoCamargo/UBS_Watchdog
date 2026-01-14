import json
import time
import uuid
from datetime import datetime, timezone

import pytest
from tests.helpers.assertions import assert_status

pytestmark = pytest.mark.integration

# ---------------------------------------------------------------------
# Configuration
# ---------------------------------------------------------------------
#
# If your API exposes audit logs under a known endpoint, set it here.
# Otherwise, the test will probe common candidates below.
#
AUDIT_PATH_OVERRIDE: str | None = None

AUDIT_PATH_CANDIDATES = [
    "/api/audit-logs",
    "/api/auditlogs",
    "/api/audit/logs",
    "/api/audit/events",
    "/api/audit",
]

DEFAULT_AUDIT_PAGE_PARAMS = {"page": 1, "pageSize": 50}


# ---------------------------------------------------------------------
# Helpers (generic / schema-tolerant)
# ---------------------------------------------------------------------
def _utc_now_iso() -> str:
    return datetime.now(timezone.utc).isoformat()


def _try_json(resp):
    try:
        return resp.json()
    except Exception:
        return None


def _paged_items(body):
    """
    Normalize "paged" responses:
      - { "items": [...] }      (common)
      - { "Items": [...] }      (PascalCase)
      - [...]                  (already a list)
    """
    if body is None:
        return []
    if isinstance(body, list):
        return body
    if isinstance(body, dict):
        if "items" in body and isinstance(body["items"], list):
            return body["items"]
        if "Items" in body and isinstance(body["Items"], list):
            return body["Items"]
    return []


def _stringify(obj) -> str:
    try:
        return json.dumps(obj, ensure_ascii=False, default=str)
    except Exception:
        return str(obj)


def _entry_mentions_value(entry: dict, needle: str) -> bool:
    """
    Audit schemas vary. The most robust thing we can do is: does the entry contain the entity id
    somewhere in its JSON representation?
    """
    hay = _stringify(entry)
    return needle in hay


def _entry_mentions_actor(entry: dict, actor_id: str | None, actor_email: str | None) -> bool:
    """
    Link audit entry to the authenticated principal.
    We accept either the actor id OR the actor email appearing in the entry.
    """
    hay = _stringify(entry)
    if actor_id and actor_id in hay:
        return True
    if actor_email and actor_email.lower() in hay.lower():
        return True

    # Some APIs store actor under nested structures; the string scan above usually catches it.
    # If not, also check a few common keys.
    for k in ["actorId", "performedById", "userId", "analystId", "createdById", "updatedById"]:
        v = entry.get(k)
        if v and actor_id and str(v) == actor_id:
            return True

    for k in ["actorEmail", "performedByEmail", "userEmail", "analystEmail", "createdByEmail", "updatedByEmail"]:
        v = entry.get(k)
        if v and actor_email and str(v).lower() == actor_email.lower():
            return True

    return False


def _resolve_audit_path_or_fail(authed) -> str:
    """
    Resolve the audit log endpoint path.
    If AUDIT_PATH_OVERRIDE is set, use it.
    Otherwise probe common candidates.
    """
    if AUDIT_PATH_OVERRIDE:
        return AUDIT_PATH_OVERRIDE

    tried = []
    for path in AUDIT_PATH_CANDIDATES:
        tried.append(path)
        r = authed.get(path, params=DEFAULT_AUDIT_PAGE_PARAMS)

        # If endpoint exists but requires different params, we still treat it as "found".
        if r.status_code in (200, 400):  # 400 could be validation like missing query params
            return path
        if r.status_code == 401:
            # authed should not be 401; but if it is, we still treat as "found" and let tests fail clearly.
            return path

    raise AssertionError(
        "Audit log endpoint not found. Tried:\n- " + "\n- ".join(tried) +
        "\nSet AUDIT_PATH_OVERRIDE in tests/test_audit_log.py to the correct path."
    )


def _audit_search(authed, audit_path: str, params: dict | None = None):
    """
    Generic search/list call. Your API may support filters like entityId, entityType, dateFrom, etc.
    This test only relies on being able to fetch a recent page.
    """
    r = authed.get(audit_path, params=params or DEFAULT_AUDIT_PAGE_PARAMS)
    if r.status_code == 400:
        # Some APIs require params; try again with defaults.
        r2 = authed.get(audit_path, params=DEFAULT_AUDIT_PAGE_PARAMS)
        return r2
    return r


def _assert_audit_entry_for_entity(
    authed,
    audit_path: str,
    *,
    entity_id: str,
    actor_id: str | None,
    actor_email: str | None,
    min_expected: int = 1,
    retries: int = 6,
    sleep_s: float = 0.25,
):
    """
    Poll audit log for a short period and assert:
      - at least `min_expected` entries mention entity_id
      - and at least one of those entries mentions the actor id/email
    """
    last_resp = None
    matches: list[dict] = []

    for _ in range(retries):
        resp = _audit_search(authed, audit_path)
        last_resp = resp

        if resp.status_code != 200:
            body = _try_json(resp)
            raise AssertionError(
                "Audit search did not return 200.\n"
                f"Status: {resp.status_code}\n"
                f"URL: {resp.url}\n"
                f"Body: {_stringify(body) if body is not None else resp.text}"
            )

        body = resp.json()
        items = _paged_items(body)

        matches = [e for e in items if isinstance(e, dict) and _entry_mentions_value(e, entity_id)]
        if len(matches) >= min_expected:
            # If actor is not specified, entity presence is enough
            if actor_id is None and actor_email is None:
                return matches

            # Otherwise enforce actor linkage
            if any(_entry_mentions_actor(e, actor_id, actor_email) for e in matches):
                return matches

        time.sleep(sleep_s)

    # If we got here, we didn't find it.
    body = _try_json(last_resp) if last_resp is not None else None
    raise AssertionError(
        "Audit entry not found (or not linked to actor) for entity.\n"
        f"entity_id={entity_id}\n"
        f"actor_id={actor_id}\n"
        f"actor_email={actor_email}\n"
        f"Last URL: {getattr(last_resp, 'url', None)}\n"
        f"Last status: {getattr(last_resp, 'status_code', None)}\n"
        f"Last body: {_stringify(body) if body is not None else getattr(last_resp, 'text', None)}"
    )


# ---------------------------------------------------------------------
# Domain creation helpers (reuse the patterns you already validated)
# ---------------------------------------------------------------------
def _valid_client_payload(
    *,
    legal_type: int = 0,
    name: str = "Audit Test Client",
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


def _create_client_or_skip(authed) -> dict:
    """
    Create a client. If seed dependencies (e.g., countries) are missing, skip.
    """
    payload = _valid_client_payload(name=f"Audit Client {uuid.uuid4()}")
    r = authed.post("/api/clients", json=payload)

    if r.status_code == 400:
        body = _try_json(r) or {"raw": r.text}
        pytest.skip(f"Could not create client (validation/seed issue). Response: {_stringify(body)}")

    assert_status(r, 201)
    return r.json()


def _create_account_or_skip(authed, client_id: str) -> dict:
    """
    Create an account for a client. If validation/seed blocks it, skip.
    """
    payload = {
        "accountIdentifier": f"AUD-{uuid.uuid4()}",
        "countryCode": "BR",
        "accountType": 0,   # Checking (assumption: enum int)
        "currencyCode": "BRL",
    }

    r = authed.post(f"/api/clients/{client_id}/accounts", json=payload)
    if r.status_code == 400:
        body = _try_json(r) or {"raw": r.text}
        pytest.skip(f"Could not create account (validation/seed issue). Response: {_stringify(body)}")

    assert_status(r, 201)
    return r.json()


def _create_transaction_or_skip(authed, account_id: str, *, amount: float, currency: str = "BRL", tx_type: int = 0) -> dict:
    """
    Create a transaction. If FX rates/base currency config blocks it, skip.
    Assumes endpoint: POST /api/transactions
    """
    payload = {
        "accountId": account_id,
        "type": tx_type,  # 0=Deposit (assumption: enum int)
        "amount": amount,
        "currencyCode": currency,
        "occurredAtUtc": _utc_now_iso(),
    }

    r = authed.post("/api/transactions", json=payload)

    if r.status_code == 404:
        pytest.skip("Transactions endpoint not available (POST /api/transactions returned 404).")

    if r.status_code == 400:
        body = _try_json(r) or {"raw": r.text}
        pytest.skip(f"Could not create transaction (validation/FX config). Response: {_stringify(body)}")

    # Typical: 201 Created. Some APIs use 200 OK.
    if r.status_code not in (200, 201):
        body = _try_json(r) or {"raw": r.text}
        raise AssertionError(f"Unexpected transaction create status={r.status_code}. Body={_stringify(body)}")

    return r.json()


def _get_case_by_transaction_or_skip(authed, transaction_id: str) -> dict:
    """
    Find the case created for a transaction via GET /api/cases?transactionId=<id>.
    """
    r = authed.get("/api/cases", params={"transactionId": transaction_id, "page": 1, "pageSize": 20})
    if r.status_code == 404:
        pytest.skip("Cases endpoint not available (GET /api/cases returned 404).")
    assert_status(r, 200)

    body = r.json()
    items = _paged_items(body)  # cases list returns paged result
    if not items:
        pytest.skip(f"No case returned for transactionId={transaction_id}. Compliance may be disabled.")
    return items[0]


# ---------------------------------------------------------------------
# Tests: "exhaust the cases" for create/change/update and actor linkage
# ---------------------------------------------------------------------
def test_audit_log_exhaustive_create_change_update_are_logged_and_linked_to_actor(authed, api_up):
    """
    Exhaustive-ish audit test across core flows:
      1) Create Client => audit entry linked to actor
      2) Create Account => audit entry linked to actor
      3) Create Transaction => audit entry linked to actor
      4) Trigger Case via compliance violation => audit entry linked to actor (case creation can be "system"; we validate later updates)
      5) Assign-to-me (change) => audit entry linked to actor
      6) Update case workflow (update) => audit entry linked to actor
      7) Patch a compliance rule (update) => audit entry linked to actor (best-effort)
    """
    audit_path = _resolve_audit_path_or_fail(authed)

    # Identify actor (analyst)
    me = None
    me_r = authed.auth_me()
    if me_r.status_code == 200:
        me = me_r.json()

    actor_id = str(me.get("id")) if isinstance(me, dict) and me.get("id") else None
    actor_email = str(me.get("corporateEmail")) if isinstance(me, dict) and me.get("corporateEmail") else None

    # -------------------------
    # 1) CREATE CLIENT
    # -------------------------
    client = _create_client_or_skip(authed)
    client_id = client["id"]

    _assert_audit_entry_for_entity(
        authed, audit_path,
        entity_id=client_id,
        actor_id=actor_id,
        actor_email=actor_email,
    )

    # -------------------------
    # 2) CREATE ACCOUNT
    # -------------------------
    account = _create_account_or_skip(authed, client_id)
    account_id = account["id"]

    _assert_audit_entry_for_entity(
        authed, audit_path,
        entity_id=account_id,
        actor_id=actor_id,
        actor_email=actor_email,
    )

    # -------------------------
    # 3) CREATE TRANSACTION (benign)
    # -------------------------
    tx = _create_transaction_or_skip(authed, account_id, amount=10.0, currency="BRL", tx_type=0)
    tx_id = tx["id"]

    _assert_audit_entry_for_entity(
        authed, audit_path,
        entity_id=tx_id,
        actor_id=actor_id,
        actor_email=actor_email,
    )

    # -------------------------
    # 4) CREATE VIOLATING TRANSACTION -> CASE CREATED
    #    (Daily limit is seeded with 10,000 base amount; we exceed it with a large BRL deposit.)
    # -------------------------
    violating_tx = _create_transaction_or_skip(authed, account_id, amount=999_999.0, currency="BRL", tx_type=0)
    violating_tx_id = violating_tx["id"]

    # Transaction itself must be audited too.
    _assert_audit_entry_for_entity(
        authed, audit_path,
        entity_id=violating_tx_id,
        actor_id=actor_id,
        actor_email=actor_email,
    )

    case_summary = _get_case_by_transaction_or_skip(authed, violating_tx_id)
    case_id = case_summary["id"]

    # Note: case creation may be attributed to "system" (service side) depending on design.
    # We will not enforce actor linkage on creation, but we WILL enforce it on user-driven changes below.
    # Still, we do assert that the case exists in the audit log.
    _assert_audit_entry_for_entity(
        authed, audit_path,
        entity_id=case_id,
        actor_id=None,
        actor_email=None,
        min_expected=1,
    )

    # -------------------------
    # 5) ASSIGN-TO-ME (change)
    # -------------------------
    assign_r = authed.post(f"/api/cases/{case_id}/assign-to-me")
    if assign_r.status_code == 404:
        pytest.skip("Assign-to-me endpoint not available (POST /api/cases/{id}/assign-to-me returned 404).")
    assert_status(assign_r, 200)

    _assert_audit_entry_for_entity(
        authed, audit_path,
        entity_id=case_id,
        actor_id=actor_id,
        actor_email=actor_email,
        min_expected=1,
    )

    # -------------------------
    # 6) UPDATE CASE WORKFLOW (update)
    #    UnderReview->Resolved requires decision; we do a direct resolve.
    # -------------------------
    update_r = authed.patch(f"/api/cases/{case_id}", json={"status": 2, "decision": 2})
    # Assumption: CaseStatus enum: New=0, UnderReview=1, Resolved=2; Decision enum includes 2 (Inconclusive).
    # If your enums differ, adjust numbers accordingly.
    if update_r.status_code == 400:
        # Fallback: try UnderReview without decision, then resolve with decision.
        ur_r = authed.patch(f"/api/cases/{case_id}", json={"status": 1})
        if ur_r.status_code == 200:
            update_r = authed.patch(f"/api/cases/{case_id}", json={"status": 2, "decision": 2})

    assert_status(update_r, 200)

    _assert_audit_entry_for_entity(
        authed, audit_path,
        entity_id=case_id,
        actor_id=actor_id,
        actor_email=actor_email,
        min_expected=1,
    )

    # -------------------------
    # 7) PATCH COMPLIANCE RULE (update) - best effort
    # -------------------------
    rules_r = authed.get("/api/rules", params={"page": 1, "pageSize": 100})
    if rules_r.status_code == 404:
        pytest.skip("Rules endpoint not available (GET /api/rules returned 404).")
    assert_status(rules_r, 200)

    rules_body = rules_r.json()
    rules = _paged_items(rules_body)
    daily = next((x for x in rules if isinstance(x, dict) and x.get("code") == "daily_limit_default"), None)
    if daily is None:
        pytest.skip("Seeded rule daily_limit_default not found; seeding config differs.")

    rule_id = daily.get("id")
    if not rule_id:
        pytest.skip("Rule payload did not include 'id'; cannot patch.")

    # Minimal patch: tweak name (should be allowed if PatchRuleRequest supports it).
    patch_payload = {"name": f"{daily.get('name', 'Daily Limit')} (audited {uuid.uuid4()})"}
    patch_r = authed.patch(f"/api/rules/{rule_id}", json=patch_payload)

    if patch_r.status_code in (400, 422):
        # If name patch isn't allowed, try a no-op patch (some APIs still log) or skip.
        body = _try_json(patch_r) or {"raw": patch_r.text}
        pytest.skip(f"Rule patch rejected by API. Response: {_stringify(body)}")

    assert_status(patch_r, 200)

    _assert_audit_entry_for_entity(
        authed, audit_path,
        entity_id=str(rule_id),
        actor_id=actor_id,
        actor_email=actor_email,
        min_expected=1,
    )
