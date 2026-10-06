---
id: I-3-plan
status: approved
---

# Plan: Local development environment with .NET Aspire

## Files

### New files
- `src/Petit.AppHost/Petit.AppHost.csproj` (new) — Aspire AppHost project file targeting net10.0, referencing the Aspire hosting packages and Petit.WebApi
- `src/Petit.AppHost/Program.cs` (new) — Aspire distributed application builder: registers `petit-postgres`, `petit-keycloak`, and `Petit.WebApi` with service discovery references
- `src/Petit.AppHost/appsettings.json` (new) — default config (no secrets)
- `src/Petit.AppHost/appsettings.Development.json` (new) — local dev overrides if needed
- `src/Petit.AppHost/keycloak/realm-export.json` (new) — Keycloak realm export with `petit` realm, `petit-api` client, and `devuser` test user

### Modified files
- `petit-net.slnx` (modified) — add `Petit.AppHost` project to the solution
- `src/Petit.WebApi/appsettings.json` (modified) — update `PetitDbConnection` database name from `petitdb` to `petit` to match Aspire's `.AddDatabase("petit")`
- `src/Petit.WebApi/appsettings.Development.json` (modified) — add `ConnectionStrings:PetitDbConnection` placeholder that Aspire will override at runtime

## Order

1. **Create the AppHost project structure** — create `src/Petit.AppHost/` directory, `Program.cs`, `appsettings.json`, and `appsettings.Development.json`. The Program.cs wires up PostgreSQL (`petit-postgres` with `.WithDataVolume().AddDatabase("petit")`), Keycloak (`petit-keycloak` with admin credentials **and** `.WithReference(db)` to use the same PostgreSQL as its backend), and the API project with `.WithReference()` for both services.

2. **Create the Keycloak realm export** — write `src/Petit.AppHost/keycloak/realm-export.json` containing:
   - Realm: `petit`
   - Client: `petit-api` (valid redirect URIs: `http://localhost:*`)
   - Test user: `devuser` / `devpassword` with role mapped to `petit-api` audience
   - Bind this file into the Keycloak container via Aspire's `WithBindMount()` so it auto-imports on first boot.

3. **Update appsettings files** — add a `ConnectionStrings:PetitDbConnection` placeholder to `appsettings.Development.json` so Aspire can inject the real value at runtime. (The database name in `appsettings.json` is already `petit`, matching `.AddDatabase("petit")`.)

4. **Add the project to the solution** — update `petit-net.slnx` to include `src/Petit.AppHost/Petit.AppHost.csproj`.

5. **Verify build and test** — run `dotnet build` and `dotnet test` to confirm nothing is broken by the new project or config changes.

## Risks
- **Aspire API surface**: The spec uses conceptual APIs (`AddPostgres`, `AddKeycloak`, `WithReference`) that may differ from the actual .NET 10 Aspire preview/stable API. The planner must verify exact package names and method signatures before implementation.
- **Keycloak PostgreSQL storage**: Keycloak now uses the shared `petit-postgres` instance as its backend (not H2). This means both the API and Keycloak share the same database — simpler for local dev but couples their lifecycles. If they need to be decoupled later, a separate Keycloak DB would be required.
- **Realm import timing**: Keycloak takes several seconds to start; the realm is imported on first boot only. If the volume is wiped, re-import is needed. A health check or startup hook could be added later if reliability becomes an issue.
- **appsettings.Development.json placeholder**: The API currently reads `Keycloak:Authority` and `Keycloak:Audience` from config with no fallback defaults in Development. Aspire will inject these via environment variables, but the local dev defaults in `appsettings.Development.json` (`http://localhost:8080/realms/petit`) must remain as a safe fallback for non-Aspire runs.
- **Database name alignment**: The database name in `appsettings.json` is already `petit`, matching Aspire's `.AddDatabase("petit")`. No further action needed here.

## Completion criteria
- All listed files exist and match their stated purpose
- `dotnet build` succeeds with the new AppHost project included in the solution
- `dotnet test` passes (no regressions from config changes)
- The AppHost can start the full stack: PostgreSQL, Keycloak, and API are all visible in the Aspire dashboard
- The realm is imported into Keycloak on first boot (verified by logging or admin console)
- The API connects to PostgreSQL using the injected `PetitDbConnection` connection string
- No hardcoded secrets exist in any committed file
