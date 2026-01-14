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


def test_login_invalid_email_format_returns_400(api, api_up, creds):
    """
    Verify that login payload validation rejects invalid email formats.

    This test ensures:
        - The login endpoint returns HTTP 400 Bad Request (ProblemDetails)
    """
    r = api.auth_login("not-an-email", creds["password"])
    assert_problem_details(r, 400)


def test_login_empty_email_returns_400(api, api_up, creds):
    """
    Verify that login payload validation rejects empty email.

    This test ensures:
        - The login endpoint returns HTTP 400 Bad Request (ProblemDetails)
    """
    r = api.auth_login("", creds["password"])
    assert_problem_details(r, 400)


def test_login_whitespace_email_returns_400(api, api_up, creds):
    """
    Verify that login payload validation rejects whitespace-only email.

    This test ensures:
        - The login endpoint returns HTTP 400 Bad Request (ProblemDetails)
    """
    r = api.auth_login("   ", creds["password"])
    assert_problem_details(r, 400)


def test_login_email_with_leading_trailing_spaces_returns_400(api, api_up, creds):
    """
    Verify that login payload validation rejects email with leading/trailing spaces.

    This test ensures:
        - The login endpoint returns HTTP 400 Bad Request (ProblemDetails)
    """
    spaced_email = f"  {creds['email'].strip()}  "
    r = api.auth_login(spaced_email, creds["password"])
    assert_problem_details(r, 400)


def test_login_empty_password_returns_400(api, api_up, creds):
    """
    Verify that login payload validation rejects empty password.

    This test ensures:
        - The login endpoint returns HTTP 400 Bad Request (ProblemDetails)
    """
    r = api.auth_login(creds["email"], "")
    assert_problem_details(r, 400)


def test_login_whitespace_password_returns_400(api, api_up, creds):
    """
    Verify that login payload validation rejects whitespace-only password.

    This test ensures:
        - The login endpoint returns HTTP 400 Bad Request (ProblemDetails)
    """
    r = api.auth_login(creds["email"], "   ")
    assert_problem_details(r, 400)


def test_login_password_too_short_returns_400(api, api_up, creds):
    """
    Verify that login payload validation enforces email min length.

    This test ensures:
        - The login endpoint returns HTTP 400 Bad Request (ProblemDetails)
    """
    r = api.auth_login(creds["email"], "Valid")
    assert_problem_details(r, 400)



def test_login_email_too_long_returns_400(api, api_up):
    """
    Verify that login payload validation enforces email max length.

    This test ensures:
        - The login endpoint returns HTTP 400 Bad Request (ProblemDetails)
    """
    # RFC-like max is 254; we send a longer one
    local = "a" * 249
    email = f"{local}@ex.com"  # > 254
    r = api.auth_login(email, "ValidPassword123!")
    assert_problem_details(r, 400)


def test_login_password_too_long_returns_400(api, api_up, creds):
    """
    Verify that login payload validation enforces password max length.

    This test ensures:
        - The login endpoint returns HTTP 400 Bad Request (ProblemDetails)
    """
    pwd = "x" * 300
    r = api.auth_login(creds["email"], pwd)
    assert_problem_details(r, 400)


def test_login_missing_body_returns_400(api, api_up):
    """
    Verify that sending no JSON body is rejected.

    This test ensures:
        - The login endpoint returns HTTP 400 Bad Request (ProblemDetails)
    """
    r = api.post_raw("/api/auth/login", data=b"", content_type="application/json")
    assert_problem_details(r, 400)


def test_login_null_body_returns_400(api, api_up):
    """
    Verify that sending JSON null is rejected by model binding / validation.

    This test ensures:
        - The login endpoint returns HTTP 400 Bad Request (ProblemDetails)
    """
    r = api.post_raw("/api/auth/login", data=b"null", content_type="application/json")
    assert_problem_details(r, 400)


def test_login_invalid_json_returns_400(api, api_up):
    """
    Verify that invalid JSON is rejected.

    This test ensures:
        - The login endpoint returns HTTP 400 Bad Request (ProblemDetails)
    """
    r = api.post_raw("/api/auth/login", data=b"{", content_type="application/json")
    assert_problem_details(r, 400)


def test_login_wrong_content_type_returns_415(api, api_up, creds):
    """
    Verify that non-JSON content type is rejected because the endpoint consumes application/json.

    This test ensures:
        - The login endpoint returns HTTP 415 Unsupported Media Type
    """
    payload = f'{{"email":"{creds["email"]}","password":"{creds["password"]}"}}'.encode("utf-8")
    r = api.post_raw("/api/auth/login", data=payload, content_type="text/plain")
    assert_status(r, 415)


def test_me_requires_auth(api, api_up):
    """
    Verify that accessing the 'me' endpoint without authentication is rejected.

    This test ensures:
        - The API responds with HTTP 401 Unauthorized
    """
    r = api.get("/api/auth/me")
    assert r.status_code == 401


def test_me_invalid_token_returns_401(api, api_up):
    """
    Verify that accessing the 'me' endpoint with an invalid token is rejected.

    This test ensures:
        - The API responds with HTTP 401 Unauthorized
    """
    r = api.get("/api/auth/me", headers={"Authorization": "Bearer invalid.token.value"})
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


def test_logout_requires_auth(api, api_up):
    """
    Verify that logout requires authentication.

    This test ensures:
        - The API responds with HTTP 401 Unauthorized
    """
    r = api.post("/api/auth/logout")
    assert r.status_code == 401


def test_logout_success_returns_204(authed, api_up):
    """
    Verify that an authenticated analyst can logout.

    This test asserts that:
        1. The endpoint returns HTTP 204 No Content
        2. The response body is empty (or absent)
    """
    r = authed.auth_logout()
    assert_status(r, 204)

    # Some servers return an empty body; both are acceptable for 204.
    assert r.text in ("", None) or len(r.content or b"") == 0
