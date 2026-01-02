import pytest
from tests.helpers.assertions import assert_status, assert_problem_details

pytestmark = pytest.mark.integration


def test_login_success(api, api_up, creds):
    """
    Verify successful authentication with valid analyst credentials.

    This test asserts that:
        1. The login endpoint returns HTTP 200
        2. A JWT token is returned and looks structurally valid
        3. Token expiration metadata is present
        4. Analyst profile information is included
        5. Sensitive fields (e.g. password hash) are not leaked

    """
    r = api.auth_login(creds["email"], creds["password"])
    assert_status(r, 200)

    data = r.json()
    assert "token" in data and isinstance(data["token"], str) and len(data["token"]) > 10
    assert "expiresAtUtc" in data
    assert "analyst" in data

    analyst = data["analyst"]
    assert "passwordHash" not in analyst
    assert analyst.get("corporateEmail") == creds["email"].strip().lower()


def test_login_invalid_password(api, api_up, creds):
    """
    Verify that authentication fails with an invalid password.

    This test ensures:
        - The login endpoint returns HTTP 401 Unauthorized

    """
    r = api.auth_login(creds["email"], "WRONG_PASSWORD")
    assert_problem_details(r, 401)


def test_me_requires_auth(api, api_up):
    """
    Verify that accessing the 'me' endpoint without authentication is rejected.

    This test ensures:
        - The API responds with HTTP 401 Unauthorized
    """
    r = api.get("/api/auth/me")
    assert r.status_code == 401


def test_me_success(authed, api_up, creds):
    """
    Verify that an authenticated analyst can retrieve their own profile.

    This test asserts that:
        1. The endpoint returns HTTP 200
        2. The returned profile matches the authenticated analyst
        3. A valid analyst ID is present
        4. Sensitive fields (e.g. password hash) are not exposed
    """
    r = authed.auth_me()
    assert_status(r, 200)

    me = r.json()
    assert me.get("corporateEmail") == creds["email"].strip().lower()
    assert me.get("id")
    assert "passwordHash" not in me
