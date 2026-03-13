# Auth

Shared auth infrastructure for ASP.NET apps using JWT + API key and DB-backed roles.

## API key authentication

When a request includes the `X-Api-Key` header and the value matches configuration:

- **Role claim**: Set to the role with `SortOrder == 0` (e.g. "Owner"). So the API key is treated as that role for display/audit.
- **Permissions**: Union of **all** roles’ permissions from the database, not just that one role. So the key has full access regardless of which role is “first”.
- **User ID claim**: `"service"` (not a real user id).

Use the API key for server-to-server calls (e.g. dashboard exchanging OAuth for JWT, bots calling the API). Require `auth.session.create` (and optionally `auth.token.create`) on auth endpoints so only API-key callers can use them; grant those permissions to the sortOrder-0 role so the key gets them.
