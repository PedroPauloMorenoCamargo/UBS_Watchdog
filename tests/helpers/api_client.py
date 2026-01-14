import requests


class ApiClient:
    """
    HTTP client wrapper used by pytest integration tests to interact with the UBS Monitoring API.
    """

    def __init__(self, base_url: str, token: str | None = None, timeout: int = 15):
        """
        Initialize the API client.

        Args:
            base_url: Root URL of the API (e.g. "http://localhost:8080").
            token: Optional JWT bearer token used for authenticated requests.
            timeout: Default timeout (in seconds) applied to all HTTP requests.
        """
        self.base_url = base_url.rstrip("/")
        self.timeout = timeout
        self.session = requests.Session()
        self.token = token

    def with_token(self, token: str) -> "ApiClient":
        """
        New ApiClient instance sharing the same base configuration but authenticated with a different JWT token.

        Useful to keep the original client immutable.

        Args:
            token: JWT bearer token.

        Returns:
            New ApiClient instance with the given token.
        """
        return ApiClient(self.base_url, token=token, timeout=self.timeout)

    def _headers(self, extra: dict | None = None) -> dict:
        """
        Build the HTTP headers for a request.

        Automatically injects:
            - Accept: application/json
            - Authorization: Bearer <token> (if token is present)

        Args:
            extra: Optional dictionary of additional headers to merge.

        Returns:
            Dictionary containing the final headers for the request.
        """
        headers = {"Accept": "application/json"}
        if self.token:
            headers["Authorization"] = f"Bearer {self.token}"
        if extra:
            headers.update(extra)
        return headers

    # -------------------------------------------------
    # Low-level request helpers
    # -------------------------------------------------

    def request(self, method: str, path: str, **kwargs):
        """
        Execute a raw HTTP request.

        This is used internally for negative test cases
        (e.g. invalid JSON, missing body, wrong content-type).
        """
        return self.session.request(
            method=method,
            url=f"{self.base_url}{path}",
            headers=self._headers(kwargs.pop("headers", None)),
            timeout=kwargs.pop("timeout", self.timeout),
            **kwargs,
        )

    def get(self, path: str, **kwargs):
        """
        Execute a HTTP GET request.

        Args:
            path: API path relative to the base URL (e.g. "/api/health").
            **kwargs: Additional arguments forwarded to requests.Session.get().

        Returns:
            requests.Response object.
        """
        return self.session.get(
            f"{self.base_url}{path}",
            headers=self._headers(kwargs.pop("headers", None)),
            timeout=kwargs.pop("timeout", self.timeout),
            **kwargs,
        )

    def post(self, path: str, **kwargs):
        """
        Execute a HTTP POST request.

        Args:
            path: API path relative to the base URL (e.g. "/api/auth/login").
            **kwargs: Additional arguments forwarded to requests.Session.post().

        Returns:
            requests.Response object.
        """
        return self.session.post(
            f"{self.base_url}{path}",
            headers=self._headers(kwargs.pop("headers", None)),
            timeout=kwargs.pop("timeout", self.timeout),
            **kwargs,
        )

    def put(self, path: str, **kwargs):
        """
        Execute an HTTP PUT request.
        """
        return self.session.put(
            f"{self.base_url}{path}",
            headers=self._headers(kwargs.pop("headers", None)),
            timeout=kwargs.pop("timeout", self.timeout),
            **kwargs,
        )

    def patch(self, path: str, **kwargs):
        """
        Execute an HTTP PATCH request.
        """
        return self.session.patch(
            f"{self.base_url}{path}",
            headers=self._headers(kwargs.pop("headers", None)),
            timeout=kwargs.pop("timeout", self.timeout),
            **kwargs,
        )

    def post_raw(self, path: str, data: bytes, content_type: str, headers=None):
        """
        Send a raw POST request, used for negative cases:
            - invalid JSON
            - missing body
            - wrong content type
        """
        h = {"Content-Type": content_type}
        if headers:
            h.update(headers)

        return self.request("POST", path, data=data, headers=h)

    # -------------------------------------------------
    # Convenience API calls
    # -------------------------------------------------

    def health(self):
        """
        Call the health endpoint to verify that API is not down.

        Endpoint:
            GET /api/health
        """
        return self.get("/api/health")

    # -----------------------------
    # Auth endpoints
    # -----------------------------

    def auth_login(self, email: str, password: str):
        """
        Authenticate an analyst and obtain a JWT token.

        Endpoint:
            POST /api/auth/login
        """
        return self.post("/api/auth/login", json={"email": email, "password": password})

    def auth_me(self):
        """
        Retrieve the profile of the currently authenticated analyst.

        Endpoint:
            GET /api/auth/me
        """
        return self.get("/api/auth/me")

    def auth_logout(self):
        """
        Logout the currently authenticated analyst.

        Endpoint:
            POST /api/auth/logout
        """
        return self.post("/api/auth/logout")

    # -----------------------------
    # Analysts endpoints
    # -----------------------------

    def analyst_get(self, analyst_id: str):
        """
        Retrieve an analyst profile by ID.

        Endpoint:
            GET /api/analysts/{id}
        """
        return self.get(f"/api/analysts/{analyst_id}")

    def analyst_update_profile_picture(self, base64_payload: str | None):
        """
        Update or clear the authenticated analyst's profile picture.

        Endpoint:
            PATCH /api/analysts/me/profile-picture
        """
        return self.patch(
            "/api/analysts/me/profile-picture",
            json={"profilePictureBase64": base64_payload},
        )

    # -----------------------------
    # Rules endpoints
    # -----------------------------

    def rules_search(self, params: dict | None = None):
        """
        Search/list compliance rules.

        Endpoint:
            GET /api/rules
        """
        return self.get("/api/rules", params=params or {})

    def rules_get(self, rule_id: str):
        """
        Retrieve a compliance rule by ID.

        Endpoint:
            GET /api/rules/{id}
        """
        return self.get(f"/api/rules/{rule_id}")

    def rules_patch(self, rule_id: str, payload: dict | None = None):
        """
        Patch/update a compliance rule.

        Endpoint:
            PATCH /api/rules/{id}
        """
        if payload is None:
            return self.patch(f"/api/rules/{rule_id}")
        return self.patch(f"/api/rules/{rule_id}", json=payload)
