---
id: I-3-local-dev-environment
status: approved
---

# Intent: Local development environment with Aspire

## Ask
A one-command local environment that runs the API together with its infrastructure dependencies — PostgreSQL and Keycloak (identity provider) — using .NET Aspire, so developers can start the full stack with a single command and manually test all endpoints including JWT-authenticated ones.

## Why
Developers currently lack a reproducible local setup to run and manually test the API. Installing and configuring PostgreSQL and Keycloak by hand is error-prone and time-consuming. Placeholder Keycloak Authority and Audience values are used in local configuration, which obscures how real authentication works during development. This intent replaces those placeholders with a working local Keycloak instance that provides real JWT tokens for testing.

## Constraints
- Use .NET Aspire (AppHost project), not docker-compose
- Container runtime: Docker
- PostgreSQL data must persist between restarts
- Keycloak must start with a pre-configured realm (imported from a JSON file in the repo) containing: an API client/audience, and at least one test user
- The API receives its database connection string and Keycloak Authority/Audience from the AppHost, not from hardcoded values
- No real secrets in the repo; local dev credentials only
- Exact package names and versions to be verified against current documentation before implementation

## Open Questions
- Should Keycloak store its data in the same PostgreSQL instance or use its own internal storage?
- Should database migrations be applied automatically on startup (e.g. via a migration container or Aspire service hook)?
- Which test users and roles/permissions are needed for manual testing of protected endpoints?
- How should developers obtain a token for manual testing (e.g. a documented `curl` command, a small helper script, or Aspire's dashboard tokens endpoint)?
