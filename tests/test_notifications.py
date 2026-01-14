import json
import queue
import time
import uuid
from datetime import datetime, timezone

import pytest
from tests.helpers.assertions import assert_status

pytestmark = pytest.mark.integration


def _now_iso_z():
    return datetime.now(timezone.utc).replace(microsecond=0).isoformat().replace("+00:00", "Z")


def _paged_items(payload: dict) -> list:
    if not isinstance(payload, dict):
        return []
    for key in ("items", "Items", "data", "Data"):
        v = payload.get(key)
        if isinstance(v, list):
            return v
    return []


def _extract_bearer_token(authed) -> str | None:
    """
    Best-effort extraction of a JWT from the authed ApiClient, without assuming its internal shape.
    """
    # Common attribute names
    for attr in ("token", "access_token", "jwt", "_token", "_jwt", "bearer_token"):
        if hasattr(authed, attr):
            v = getattr(authed, attr)
            if isinstance(v, str) and v.strip():
                return v.strip()

    # Try requests.Session headers
    for attr in ("session", "_session"):
        if hasattr(authed, attr):
            s = getattr(authed, attr)
            hdrs = getattr(s, "headers", None)
            if isinstance(hdrs, dict):
                auth = hdrs.get("Authorization") or hdrs.get("authorization")
                if isinstance(auth, str) and auth.lower().startswith("bearer "):
                    return auth.split(" ", 1)[1].strip()

    return None


def _base_url(authed) -> str:
    for attr in ("base_url", "BaseUrl", "_base_url"):
        if hasattr(authed, attr):
            v = getattr(authed, attr)
            if isinstance(v, str) and v.strip():
                return v.strip().rstrip("/")
    # Default for local runs
    return "http://localhost:8080"


def _get_rule_by_code_or_skip(authed, code: str) -> dict:
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


def _valid_client_payload(*, name: str):
    return {
        "legalType": 0,
        "name": name,
        "contactNumber": "+5511999999999",
        "addressJson": {
            "street": "Av Paulista",
            "city": "São Paulo",
            "state": "SP",
            "zipCode": "01310-100",
            "country": "Brazil",
        },
        "countryCode": "BR",
        "initialRiskLevel": 1,
    }


def _create_client_or_skip(authed) -> dict:
    payload = _valid_client_payload(name=f"NotifClient-{uuid.uuid4()}")
    r = authed.client_create(payload)
    if r.status_code == 400:
        try:
            body = r.json()
        except Exception:
            body = {"raw": r.text}
        pytest.skip(f"Could not create client. Response: {json.dumps(body, ensure_ascii=False)}")
    assert_status(r, 201)
    return r.json()


def _create_account_or_skip(authed, client_id: str) -> dict:
    payload = {
        "accountIdentifier": f"ACC-{uuid.uuid4()}",
        "countryCode": "BR",
        "accountType": 0,
        "currencyCode": "BRL",
    }
    r = authed.account_create(client_id, payload)
    if r.status_code == 400:
        try:
            body = r.json()
        except Exception:
            body = {"raw": r.text}
        pytest.skip(f"Could not create account. Response: {json.dumps(body, ensure_ascii=False)}")
    assert_status(r, 201)
    return r.json()


def _deposit_payload(account_id: str, *, amount: float, currency: str = "USD"):
    return {
        "accountId": account_id,
        "type": 0,  # Deposit
        "amount": amount,
        "currencyCode": currency,
        "occurredAtUtc": _now_iso_z(),
    }


def _payload_get_case_insensitive(d: dict, key: str):
    if not isinstance(d, dict):
        return None
    candidates = [
        key,
        key.lower(),
        key[:1].lower() + key[1:],
        key[:1].upper() + key[1:],
    ]
    for k in candidates:
        if k in d:
            return d[k]
    # Also try scanning by lowercase equivalence
    lk = key.lower()
    for k, v in d.items():
        if isinstance(k, str) and k.lower() == lk:
            return v
    return None


def test_signalr_case_opened_notification_is_received_without_transaction_id(authed, api_up):
    """
    Connect to SignalR hub and assert we receive `caseOpened` notification after a compliance violation.

    Assumption: notification payload may NOT contain transactionId (do not assert it).
    We assert at least: caseId, clientId, accountId, severity, openedAtUtc.
    """
    signalrcore = pytest.importorskip("signalrcore")
    from signalrcore.hub_connection_builder import HubConnectionBuilder

    # Ensure the seeded rule exists (environment sanity)
    _get_rule_by_code_or_skip(authed, "daily_limit_default")

    token = _extract_bearer_token(authed)
    if not token:
        pytest.skip("Could not extract JWT from authed client; cannot authenticate SignalR connection.")

    base = _base_url(authed)

    # IMPORTANT:
    # If you mapped the hub as "/hubs/cases", keep it.
    # If you mapped it under "/api/hubs/cases", change here accordingly.
    hub_url = f"{base}/hubs/cases"

    # Capture notifications from SignalR callback thread
    received = queue.Queue()

    # Build SignalR connection (JWT via access_token)
    hub = (
        HubConnectionBuilder()
        .with_url(hub_url, options={"access_token_factory": (lambda: token)})
        .with_automatic_reconnect({"type": "raw", "keep_alive_interval": 10, "reconnect_interval": 2})
        .build()
    )

    def _on_case_opened(args):
        # signalrcore passes args as a list of hub method arguments
        payload = None
        if isinstance(args, list) and args:
            payload = args[0]
        else:
            payload = args
        received.put(payload)

    # Subscribe
    hub.on("caseOpened", _on_case_opened)

    # Start connection
    start_error = {"err": None}

    def _on_error(err):
        start_error["err"] = err

    hub.on_error(_on_error)

    try:
        hub.start()
        # Give the hub a moment to connect and join groups (OnConnectedAsync)
        time.sleep(0.5)

        if start_error["err"] is not None:
            pytest.fail(f"SignalR connection error: {start_error['err']}")

        # Create data and trigger case creation
        client = _create_client_or_skip(authed)
        account = _create_account_or_skip(authed, client["id"])

        # Large deposit intended to exceed daily limit
        tx_resp = authed.transaction_create(_deposit_payload(account["id"], amount=25000.0, currency="USD"))
        if tx_resp.status_code == 400:
            try:
                body = tx_resp.json()
            except Exception:
                body = {"raw": tx_resp.text}
            pytest.skip(f"Could not create violating transaction (env differs). Response: {json.dumps(body, ensure_ascii=False)}")

        assert_status(tx_resp, 201)

        # Wait for notification
        try:
            payload = received.get(timeout=10)
        except queue.Empty:
            pytest.fail(
                "Did not receive SignalR `caseOpened` notification within 10s. "
                f"Check hub mapping ({hub_url}), auth/JWT on hub, and that publisher sends to Group('analysts')."
            )

        # Validate payload shape/content (transactionId is intentionally NOT asserted)
        assert isinstance(payload, dict), f"Expected dict payload, got {type(payload)}: {payload}"

        case_id = _payload_get_case_insensitive(payload, "caseId")
        client_id = _payload_get_case_insensitive(payload, "clientId")
        account_id = _payload_get_case_insensitive(payload, "accountId")
        severity = _payload_get_case_insensitive(payload, "severity")
        opened_at = _payload_get_case_insensitive(payload, "openedAtUtc")

        assert case_id, f"Missing caseId in payload: {payload}"
        assert client_id == client["id"], f"Expected clientId={client['id']}, got {client_id}"
        assert account_id == account["id"], f"Expected accountId={account['id']}, got {account_id}"
        assert severity is not None, f"Missing severity in payload: {payload}"
        assert opened_at, f"Missing openedAtUtc in payload: {payload}"

        # Validate caseId looks like UUID (best-effort)
        uuid.UUID(str(case_id))

    finally:
        try:
            hub.stop()
        except Exception:
            pass
