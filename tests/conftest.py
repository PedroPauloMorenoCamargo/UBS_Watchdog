import os
import pytest
from requests import RequestException

from tests.helpers.api_client import ApiClient
from tests.helpers.assertions import assert_status


def pytest_addoption(parser):
    """
    Register custom command-line options for pytest.

    Examples:
        pytest --base-url http://localhost:8080
        pytest --base-url https://staging.api.company.com

    The option can also be provided via the API_BASE_URL environment variable.
    """
    parser.addoption(
        "--base-url",
        action="store",
        default=os.getenv("API_BASE_URL", "http://localhost:8080"),
        help="API base URL (default from API_BASE_URL or http://localhost:8080)",
    )


@pytest.fixture(scope="session")
def base_url(pytestconfig):
    """
    Resolve the base URL for the API under test.

    Priority:
        1. --base-url pytest command-line argument
        2. API_BASE_URL environment variable
        3. Default: http://localhost:8080

    Returns:
        Normalized base URL string without trailing slash.
    """
    return pytestconfig.getoption("--base-url").rstrip("/")


@pytest.fixture(scope="session")
def creds():
    """
    Provide credentials for the test analyst account.

    Credentials can be overridden using environment variables:
        - TEST_ANALYST_EMAIL
        - TEST_ANALYST_PASSWORD

    Returns:
        Dictionary containing email and password.
    """
    return {
        "email": os.getenv("TEST_ANALYST_EMAIL", "analyst@ubs.com"),
        "password": os.getenv("TEST_ANALYST_PASSWORD", "Password123!"),
    }


@pytest.fixture(scope="session")
def api(base_url):
    """
    Create a unauthenticated ApiClient instance.

    This client is used for:
        - health checks
        - login
        - endpoints that do not require authentication

    Returns:
        ApiClient without JWT token.
    """
    return ApiClient(base_url)


@pytest.fixture(scope="session")
def api_up(api):
    """
    Check wether the API is reachable before running any integration tests.

    This fixture:
        - Calls the health endpoint
        - Fails the entire test session early if the API is down
        - Prevents misleading failures in downstream tests

    Returns:
        True if the API is reachable.

    Raises:
        RuntimeError: If the API cannot be reached or does not respond correctly.
    """
    try:
        r = api.health()
        assert_status(r, 200)
    except RequestException as ex:
        raise RuntimeError(
            "API is not reachable. Start the API and retry.\n"
            f"BASE_URL={api.base_url}\n"
            f"Error: {ex}"
        ) from ex
    return True


@pytest.fixture(scope="session")
def token(api, api_up, creds):
    """
    Authenticate using the test analyst credentials and obtain a JWT token.

    This fixture depends on:
        - api_up: ensures the API is alive
        - creds: provides login credentials

    Returns:
        JWT token string.

    Raises:
        AssertionError: If login fails or no token is returned.
    """
    r = api.auth_login(creds["email"], creds["password"])
    assert_status(r, 200)

    data = r.json()
    t = data.get("token")
    if not t:
        raise AssertionError(f"Login did not return token. Body={data}")

    return t


@pytest.fixture(scope="session")
def authed(api, token):
    """
    Create an authenticated ApiClient using the JWT token.

    This client is used for all endpoints that require authentication.

    Returns:
        ApiClient instance with Authorization header configured.
    """
    return api.with_token(token)


@pytest.fixture(scope="session")
def me(authed, api_up):
    """
    Retrieve the profile of the authenticated analyst.

    This fixture is commonly reused across tests that need:
        - analyst ID
        - analyst metadata
        - validation of authentication state

    Returns:
        Parsed JSON dictionary representing the authenticated analyst.
    """
    r = authed.auth_me()
    assert_status(r, 200)
    return r.json()
