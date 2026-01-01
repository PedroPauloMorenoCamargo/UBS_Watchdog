import base64
import pytest
from tests.helpers.assertions import assert_status, assert_problem_details

pytestmark = pytest.mark.integration


def test_get_analyst_by_id(authed, api_up, me):
    """
    Verify that an authenticated analyst can retrieve an analyst profile by ID.

    This test asserts that:
        1. The endpoint returns HTTP 200
        2. The returned analyst ID matches the requested ID
        3. Sensitive fields (e.g. password hash) are not exposed
    """
    analyst_id = me["id"]
    r = authed.analyst_get(analyst_id)
    assert_status(r, 200)

    body = r.json()
    assert body["id"] == analyst_id
    assert "passwordHash" not in body


def test_update_profile_picture_valid_base64(authed, api_up):
    """
    Verify that an analyst can update their profile picture with valid Base64 input.

    This test asserts that:
        1. The response status is 200 OK or 204 No Content
        2. The updated profile picture is reflected in the /auth/me endpoint
    """
    payload = base64.b64encode(b"hello-world").decode("ascii")

    r = authed.analyst_update_profile_picture(payload)
    assert r.status_code in (204, 200)

    me2 = authed.auth_me().json()
    assert me2.get("profilePictureBase64")


def test_update_profile_picture_invalid_base64(authed, api_up):
    """
    Verify that invalid Base64 input is rejected when updating the profile picture.

    This test asserts that:
        - The endpoint returns HTTP 400 Bad Request
    """
    r = authed.analyst_update_profile_picture("NOT_BASE64!!!")
    assert_problem_details(r, 400)


def test_update_profile_picture_too_large(authed, api_up):
    """
    Verify that profile picture uploads exceeding the size limit are rejected.

    This test asserts that:
        1. The endpoint returns HTTP 400 Bad Request
    """
    # 2MB + 1 byte -> should exceed backend limit
    big = b"a" * (2 * 1024 * 1024 + 1)
    payload = base64.b64encode(big).decode("ascii")

    r = authed.analyst_update_profile_picture(payload)
    assert_problem_details(r, 400)


def test_update_profile_picture_clear(authed, api_up):
    """
    Verify that an analyst can clear their profile picture.

    This test asserts that:
        1. The endpoint returns HTTP 200 OK or 204 No Content
        2. The profile picture field is null or empty in subsequent profile reads
    """
    r = authed.analyst_update_profile_picture(None)
    assert r.status_code in (204, 200)

    me2 = authed.auth_me().json()
    assert me2.get("profilePictureBase64") in (None, "")
