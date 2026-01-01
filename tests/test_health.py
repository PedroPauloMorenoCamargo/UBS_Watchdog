import pytest
from tests.helpers.assertions import assert_status

pytestmark = pytest.mark.integration


def test_health_ok(api, api_up):
    """
    Check if the API is not Down.

    This test validates:
        1. The HTTP response status is 200 OK
        2. The response body contains the expected structure with a `"status": "ok"` field
    """
    r = api.health()
    assert_status(r, 200)

    body = r.json()
    assert body.get("status") == "ok"
