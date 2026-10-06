---
id: I-3-local-dev-environment
status: approved
---

# Spec: Local development environment with .NET Aspire

## Requirements
- R1: Developers can start the full local stack (API + PostgreSQL + Keycloak) with a single command via an Aspire AppHost project.
- R2: PostgreSQL data must persist between restarts using a named Docker volume managed by Aspire.
- R3: Keycloak starts with a pre-configured realm imported from a JSON file in the repo, containing at least one API client/audience and one test user.
- R4: The API receives its database connection string and Keycloak Authority/Audience from the AppHost via Aspire's service discovery / environment injection — no hardcoded values.
- R5: No real secrets in the repo; all local dev credentials are dummy values only.

## Design

### New project: `src/Petit.AppHost/` (Aspire AppHost)

A new .NET 10 class library project that serves as the Aspire orchestration entry point. It references `Petit.WebApi` and adds PostgreSQL and Keycloak as Aspire services.

```
src/Petit.AppHost/
  Program.cs          — Aspire app builder: registers WebApp, PostgreSQL, Keycloak
  appsettings.json    — default config (no secrets)
  appsettings.Development.json — local dev overrides if needed
```

**`Program.cs` structure:**

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL with persistent storage
var db = builder.AddPostgres("petit-postgres")
    .WithDataVolume()          // named volume for persistence across restarts
    .AddDatabase("petit");      // creates the petit database on first run

// Keycloak with realm import, backed by PostgreSQL
var keycloak = builder.AddKeycloak("petit-keycloak")
    .WithReference(db)              // connect Keycloak to the same PostgreSQL instance
    .WithEnvironment("KEYCLOAK_ADMIN", "admin")
    .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", "admin");  // local dev only, no real secrets

// API project — connected to PostgreSQL and Keycloak via Aspire service discovery
var api = builder.AddProject<Projects.Petit_WebApi>("api")
    .WithReference(db)
    .WithReference(keycloak);

using var app = builder.Build();
await app.RunAsync();
```

- `WithReference(db)` injects the PostgreSQL connection string into the API's configuration as a named connection string (`PetitDbConnection`), matching the key already used in `appsettings.json`.
- `WithReference(keycloak)` injects Keycloak's runtime URL so the API can resolve Authority/Audience without hardcoding.

### Keycloak realm import

A JSON realm export file placed at `src/Petit.AppHost/keycloak/realm-export.json` (or similar path under the AppHost project). This file is committed to the repo and contains:

- A realm named `petit`
- An API client with client ID `petit-api` and valid redirect URIs set to `http://localhost:*` for local dev
- At least one test user (e.g. `devuser` / `devpassword`) with a role mapped to the `petit-api` audience

The AppHost project copies this file into the Keycloak container at startup using Aspire's `WithBindMount()` or `WithVolume()` mechanism so Keycloak auto-imports it on first boot.

### API configuration changes

**`appsettings.Development.json`** — replace placeholder values with keys that will be overridden by Aspire:

```json
{
  "Keycloak": {
    "Authority": "http://localhost:8080/realms/petit",
    "Audience": "petit-api"
  }
}
```

These values are the *local dev defaults* (matching what Keycloak will expose when Aspire runs). When the AppHost runs, Aspire overrides them with the actual container URLs via environment variables. In production or CI, these would be set via other configuration sources.

**`Program.cs`** — no structural changes needed. The existing `AddJwtBearer()` setup already reads from `Keycloak:Authority` and `Keycloak:Audience` config keys, which Aspire will inject at runtime.

### Running the stack

Developers run:

```bash
dotnet run --project src/Petit.AppHost/Petit.AppHost.csproj
```

This starts:
1. PostgreSQL container (persistent volume)
2. Keycloak container (with realm imported)
3. The API project, connected to both

All three are visible in the Aspire dashboard (default `https://localhost:19888`).

### Token acquisition for manual testing

After the stack is running, developers obtain a JWT token via the Keycloak admin console or a documented `curl` command against the local Keycloak token endpoint:

```bash
curl -X POST http://localhost:8080/realms/petit/protocol/openid-connect/token \
  -d "grant_type=password" \
  -d "client_id=petit-api" \
  -d "username=devuser" \
  -d "password=devpassword"
```

This is documented in the repo (e.g. a note in `README.md` or a `.http` file) — not implemented as code.

## Constraints
- Use .NET Aspire (AppHost project), not docker-compose (from intent.md)
- Container runtime: Docker (from intent.md)
- PostgreSQL data must persist between restarts (from intent.md)
- Keycloak must start with a pre-configured realm imported from a JSON file in the repo (from intent.md)
- The API receives its database connection string and Keycloak Authority/Audience from the AppHost, not from hardcoded values (from intent.md)
- No real secrets in the repo; local dev credentials only (from intent.md)
- Exact package names and versions to be verified against current documentation before implementation (from intent.md)

## ⚠️ Flagged concerns
1. **Keycloak data storage**: The intent had an open question on whether Keycloak should use its own internal storage or the same PostgreSQL instance. This spec resolves it by connecting Keycloak to the shared `petit-postgres` via `.WithReference(db)` — Keycloak uses PostgreSQL as its backend (`--db=postgres`). This means both the API and Keycloak share the same database, which simplifies local dev (one container) but couples their lifecycles. If they should be decoupled in the future, a separate Keycloak DB would be needed.

2. **Realm import timing**: Keycloak takes several seconds to start and initialize its internal database. The realm export file is imported on first boot only. If the container is recreated (volume wiped), the realm must be re-imported. This is acceptable for local dev but should be noted. A health check or startup hook could ensure the realm is ready before the API starts — flagging this as a potential reliability concern during planning.

3. **Test users and roles**: The intent has an open question on which test users/roles are needed. This spec proposes a single `devuser` with access to the `petit-api` audience. If protected endpoints require specific roles (e.g., admin vs. user), those should be added to the realm export before implementation.

4. **Automatic database migrations**: The intent has an open question on whether EF Core migrations should run automatically. This spec does not implement auto-migration — developers apply migrations manually via `dotnet ef database update` in the API project, which is consistent with the existing CONSTITUTION.md migration rule (`dotnet ef migrations add`). Flagging this for confirmation during planning.

5. **Aspire package versions**: The intent explicitly says "exact package names and versions to be verified against current documentation before implementation." This spec uses conceptual Aspire APIs (`AddPostgres`, `AddKeycloak`, `WithReference`) — the planner must confirm these match the actual .NET 10 Aspire preview/stable API surface.

6. **No changes to CONSTITUTION.md patterns**: This intent adds infrastructure (AppHost project) but does not change any production code architecture. No forbidden patterns are introduced. The existing Minimal API, handler, and validation structure in `Petit.WebApi` remains untouched.
