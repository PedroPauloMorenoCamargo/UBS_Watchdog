import json


def _try_json(resp):
    """
    Attempt to deserialize a requests.Response body as JSON.

    Args:
        resp: requests.Response object.

    Returns:
        Parsed JSON object (dict/list) if successful, otherwise None.
    """
    try:
        return resp.json()
    except Exception:
        return None


def assert_status(resp, expected_status: int):
    """
    Assert that a HTTP response has the expected status code.

    If the assertion fails, raises an AssertionError containing a detailed, human-readable diagnostic message including:
        1. Actual vs expected status code
        2. Request URL
        3. Response headers
        4. Response body (pretty-printed JSON when possible)

    Args:
        resp: requests.Response object.
        expected_status: Expected HTTP status code (e.g. 200, 401, 404).

    Raises:
        AssertionError: If the response status code does not match.
    """
    if resp.status_code != expected_status:
        body = _try_json(resp)
        raise AssertionError(
            f"Expected HTTP {expected_status}, got {resp.status_code}\n"
            f"URL: {resp.url}\n"
            f"Headers: {dict(resp.headers)}\n"
            f"Body: {json.dumps(body, indent=2) if body is not None else resp.text}"
        )


def assert_problem_details(resp, expected_status: int):
    """
    Assert that a HTTP response matches the RFC 7807 "Problem Details" format (application/problem+json).

    The function verifies:
        1. Correct HTTP status code
        2. Content-Type is JSON or problem+json
        3. Response body is a JSON object
        4. Required ProblemDetails fields are present
        5. Embedded status matches the HTTP status

    Args:
        resp: requests.Response object.
        expected_status: Expected HTTP status code.

    Returns:
        Parsed ProblemDetails JSON object.

    Raises:
        AssertionError: If any validation step fails.
    """
    assert_status(resp, expected_status)

    ct = resp.headers.get("Content-Type", "")
    if "application/problem+json" not in ct and "application/json" not in ct:
        raise AssertionError(
            f"Expected problem+json or json, got Content-Type: {ct}"
        )

    body = _try_json(resp)
    if not isinstance(body, dict):
        raise AssertionError(
            f"Expected JSON object for ProblemDetails. Body: {resp.text}"
        )

    # Common RFC 7807 fields: type, title, status, detail, instance
    if "title" not in body or "status" not in body:
        raise AssertionError(
            f"Expected ProblemDetails-like payload, got: {body}"
        )

    if body.get("status") != expected_status:
        raise AssertionError(
            f"ProblemDetails status mismatch. "
            f"Expected {expected_status}, got {body.get('status')}"
        )

    return body
