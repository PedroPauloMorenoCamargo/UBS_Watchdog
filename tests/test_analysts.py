import base64
import pytest
from tests.helpers.assertions import assert_status, assert_problem_details

pytestmark = pytest.mark.integration


def _png_1x1_base64() -> str:
    """
    Return a minimal valid 1x1 PNG encoded in base64.

    This is used to satisfy validators that require image-like content.
    """
    # 1x1 transparent PNG
    png_bytes = base64.b64decode(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR4nGNgYGBgAAAABQAB"
        "JACfMwAAAABJRU5ErkJggg=="
    )
    return base64.b64encode(png_bytes).decode("ascii")


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


def test_get_analyst_by_id_not_found_returns_404_problem_details(authed, api_up):
    """
    Verify that requesting a non-existent analyst returns 404.

    This test ensures:
        - The endpoint returns HTTP 404 Not Found (ProblemDetails)
    """
    r = authed.analyst_get("00000000-0000-0000-0000-000000000000")
    assert_problem_details(r, 404)


def test_get_analyst_by_id_requires_auth(api, api_up):
    """
    Verify that accessing the analysts endpoint without authentication is rejected.

    This test ensures:
        - The API responds with HTTP 401 Unauthorized
    """
    r = api.analyst_get("00000000-0000-0000-0000-000000000000")
    assert r.status_code == 401


def test_update_profile_picture_valid_image_base64(authed, api_up):
    """
    Verify that an analyst can update their profile picture with valid image Base64.

    This test asserts that:
        1. The response status is 204 No Content (or 200 OK depending on your implementation)
        2. The updated profile picture is reflected in the /auth/me endpoint
    """
    payload = _png_1x1_base64()

    r = authed.analyst_update_profile_picture(payload)
    assert r.status_code in (204, 200)

    me2 = authed.auth_me().json()
    assert me2.get("profilePictureBase64")
    assert me2["profilePictureBase64"] == payload


def test_update_profile_picture_valid_data_uri(authed, api_up):
    """
    Verify that an analyst can update their profile picture using a valid data URI.

    This test asserts that:
        1. The response status is 204 No Content (or 200 OK)
        2. The updated profile picture is reflected in the /auth/me endpoint
    """
    payload = _png_1x1_base64()
    data_uri = f"data:image/png;base64,{payload}"

    r = authed.analyst_update_profile_picture(data_uri)
    assert r.status_code in (204, 200)

    me2 = authed.auth_me().json()
    assert me2.get("profilePictureBase64")
    # Depending on your service logic, you might store the raw base64 or the full data URI.
    # If you normalize to raw base64, change this assertion accordingly.
    assert me2["profilePictureBase64"] in (data_uri, payload)


def test_update_profile_picture_invalid_base64(authed, api_up):
    """
    Verify that invalid Base64 input is rejected when updating the profile picture.

    This test asserts that:
        - The endpoint returns HTTP 400 Bad Request (ProblemDetails or ValidationProblemDetails)
    """
    r = authed.analyst_update_profile_picture("NOT_BASE64!!!")
    assert_problem_details(r, 400)


def test_update_profile_picture_wrong_data_uri_mime_rejected(authed, api_up):
    """
    Verify that non-image data URIs are rejected.

    This test asserts that:
        - The endpoint returns HTTP 400 Bad Request
    """
    payload = base64.b64encode(b"hello").decode("ascii")
    r = authed.analyst_update_profile_picture(f"data:text/plain;base64,{payload}")
    assert_problem_details(r, 400)


def test_update_profile_picture_too_large(authed, api_up):
    """
    Verify that profile picture uploads exceeding the size limit are rejected.

    This test asserts that:
        - The endpoint returns HTTP 400 Bad Request
    """
    # 2MB + 1 byte -> should exceed backend limit after decoding
    big = b"a" * (2 * 1024 * 1024 + 1)
    payload = base64.b64encode(big).decode("ascii")

    r = authed.analyst_update_profile_picture(payload)
    assert_problem_details(r, 400)


def test_update_profile_picture_clear(authed, api_up):
    """
    Verify that an analyst can clear their profile picture.

    This test asserts that:
        1. The endpoint returns HTTP 204 No Content (or 200 OK)
        2. The profile picture field is null or empty in subsequent profile reads
    """
    r = authed.analyst_update_profile_picture(None)
    assert r.status_code in (204, 200)

    me2 = authed.auth_me().json()
    assert me2.get("profilePictureBase64") in (None, "")
