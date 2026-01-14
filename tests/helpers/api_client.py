import requests
import io


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
        Endpoint:
            GET /api/health
        """
        return self.get("/api/health")

    # -----------------------------
    # Auth endpoints
    # -----------------------------

    def auth_login(self, email: str, password: str):
        """
        Endpoint:
            POST /api/auth/login
        """
        return self.post("/api/auth/login", json={"email": email, "password": password})

    def auth_me(self):
        """
        Endpoint:
            GET /api/auth/me
        """
        return self.get("/api/auth/me")

    def auth_logout(self):
        """
        Endpoint:
            POST /api/auth/logout
        """
        return self.post("/api/auth/logout")

    # -----------------------------
    # Analysts endpoints
    # -----------------------------

    def analyst_get(self, analyst_id: str):
        """
        Endpoint:
            GET /api/analysts/{id}
        """
        return self.get(f"/api/analysts/{analyst_id}")

    def analyst_update_profile_picture(self, base64_payload: str | None):
        """
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
        Endpoint:
            GET /api/rules
        """
        return self.get("/api/rules", params=params or {})

    def rules_get(self, rule_id: str):
        """
        Endpoint:
            GET /api/rules/{id}
        """
        return self.get(f"/api/rules/{rule_id}")

    def rules_patch(self, rule_id: str, payload: dict | None = None):
        """
        Endpoint:
            PATCH /api/rules/{id}
        """
        if payload is None:
            return self.patch(f"/api/rules/{rule_id}")
        return self.patch(f"/api/rules/{rule_id}", json=payload)

    # -----------------------------
    # Clients endpoints
    # -----------------------------

    def client_create(self, payload: dict):
        """
        Endpoint:
            POST /api/clients
        """
        return self.post("/api/clients", json=payload)

    def clients_search(self, params: dict | None = None):
        """
        Endpoint:
            GET /api/clients
        """
        return self.get("/api/clients", params=params or {})

    def client_get(self, client_id: str):
        """
        Endpoint:
            GET /api/clients/{id}
        """
        return self.get(f"/api/clients/{client_id}")

    def clients_import(self, file_name: str, file_bytes: bytes, content_type: str = "text/csv"):
        """
        Endpoint:
            POST /api/clients/import (multipart form-data)
        """
        files = {
            "file": (file_name, io.BytesIO(file_bytes), content_type),
        }
        return self.post("/api/clients/import", files=files)

    # -------- Accounts --------

    def account_create(self, client_id: str, payload: dict):
        """
        Endpoint:
            POST /api/clients/{clientId}/accounts
        """
        return self.post(f"/api/clients/{client_id}/accounts", json=payload)

    def accounts_get_by_client(self, client_id: str):
        """
        Endpoint:
            GET /api/clients/{clientId}/accounts
        """
        return self.get(f"/api/clients/{client_id}/accounts")

    def account_get(self, account_id: str):
        """
        Endpoint:
            GET /api/accounts/{accountId}
        """
        return self.get(f"/api/accounts/{account_id}")

    def accounts_import(self, client_id: str, file_name: str, file_bytes: bytes):
        """
        Endpoint:
            POST /api/clients/{clientId}/accounts/import (multipart form-data)
        """
        files = {
            "file": (file_name, io.BytesIO(file_bytes), "text/csv"),
        }
        return self.post(f"/api/clients/{client_id}/accounts/import", files=files)
    
    # -------- Account Identifiers --------
    def account_identifiers_get_by_account(self, account_id: str):
        # GET /api/accounts/{accountId}/identifiers
        return self.get(f"/api/accounts/{account_id}/identifiers")

    def account_identifier_create(self, account_id: str, payload: dict):
        # POST /api/accounts/{accountId}/identifiers
        return self.post(f"/api/accounts/{account_id}/identifiers", json=payload)

    def account_identifier_delete(self, identifier_id: str):
        # DELETE /api/account-identifiers/{identifierId}
        return self.request("DELETE", f"/api/account-identifiers/{identifier_id}")
    
        # -------- Transactions --------
    def transaction_create(self, payload: dict):
        # POST /api/transactions
        return self.post("/api/transactions", json=payload)

    def transactions_search(self, params: dict | None = None):
        # GET /api/transactions
        return self.get("/api/transactions", params=params or {})

    def transaction_get(self, transaction_id: str):
        # GET /api/transactions/{transactionId}
        return self.get(f"/api/transactions/{transaction_id}")

    def transactions_import(self, file_name: str, file_bytes: bytes, content_type: str = "text/csv"):
        # POST /api/transactions/import (multipart form-data)
        files = {"file": (file_name, io.BytesIO(file_bytes), content_type)}
        return self.post("/api/transactions/import", files=files)
    # -------- Cases --------
    def cases_search(self, params: dict | None = None):
        # GET /api/cases
        return self.get("/api/cases", params=params or {})

    def case_get(self, case_id: str):
        # GET /api/cases/{id}
        return self.get(f"/api/cases/{case_id}")

    def case_findings(self, case_id: str, params: dict | None = None):
        # GET /api/cases/{caseId}/findings
        return self.get(f"/api/cases/{case_id}/findings", params=params or {})



