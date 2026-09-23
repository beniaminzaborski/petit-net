---
id: I-1-pets-management
status: approved
---

# Plan: Pet CRUD — full resource management

## Files

### New files
- `src/Petit.WebApi/Features/Pets/Entities/Pet.cs` — Pet entity with required/optional properties and ownership via OwnerId
- `src/Petit.WebApi/Features/Pets/Configurations/PetEntityTypeConfiguration.cs` — IEntityTypeConfiguration<T> for Pet mapping, required fields, and indexes
- `src/Petit.WebApi/AppDbContext.cs` — EF Core DbContext with `DbSet<Pet>` and configuration wiring in `OnModelCreating`.
- `src/Petit.WebApi/Program.cs` *(modified)* — add Keycloak JWT middleware, FluentValidation services, EF Core + PostgreSQL persistence, and route grouping

### Feature files (new)
- `src/Petit.WebApi/Features/Pets/RequestModels/CreatePetRequest.cs` — request model for POST /api/pets
- `src/Petit.WebApi/Features/Pets/RequestModels/UpdatePetRequest.cs` — request model for PUT /api/pets/{id}
- `src/Petit.WebApi/Features/Pets/RequestModels/PetResponse.cs` — shared response DTO

### Validators (new)
- `src/Petit.WebApi/Features/Pets/Validators/CreatePetValidator.cs` — FluentValidation rules: name required, type required, gender required
- `src/Petit.WebApi/Features/Pets/Validators/UpdatePetValidator.cs` — FluentValidation rules: all fields optional-on-update

### Handlers (new)
- `src/Petit.WebApi/Features/Pets/Handlers/CreatePetHandler.cs` — DbContext injection, CreateAsync(userId). Saves via `_context.Pets.Add()` + `_context.SaveChangesAsync()`. Returns PetResponse. Sets OwnerId from userId; never exposed to client.
- `src/Petit.WebApi/Features/Pets/Handlers/ListPetsHandler.cs` — DbContext injection, ListAsync(page, pageSize, typeFilter?, userId) with `.Where(p => p.OwnerId == userId).Select(...)` projection. Returns `(PetResponse[] data, int totalItems)`.
- `src/Petit.WebApi/Features/Pets/Handlers/GetPetHandler.cs` — DbContext injection, GetByIdAsync(id, userId). FirstOrDefaultAsync with identity and ownership filter.
- `src/Petit.WebApi/Features/Pets/Handlers/UpdatePetHandler.cs` — Fetches by id + userId, null check → 404; maps owned properties from request; calls `_context.SaveChangesAsync()`. Returns PetResponse.
- `src/Petit.WebApi/Features/Pets/Handlers/DeletePetHandler.cs` — Fetches by id + userId, null check → 404; `_context.Pets.Remove(pet); await _context.SaveChangesAsync()`

### Endpoints (new)
- `src/Petit.WebApi/Features/Pets/CreatePetEndpoint.cs` — POST /api/pets, Validates → CreatePetHandler.Create, maps to 201 + Location header. No business logic.
- `src/Petit.WebApi/Features/Pets/ListPetsEndpoint.cs` — GET /api/pets, Optional query params (page=0, pageSize=20, type), calls ListPetsHandler.ListAsync → returns paginated JSON object with data, page, pageSize, totalItems.
- `src/Petit.WebApi/Features/Pets/GetPetEndpoint.cs` — GET /api/pets/{id} → GetPetHandler.GetByIdAsync → 200 or 404 (application/problem+json).
- `src/Petit.WebApi/Features/Pets/UpdatePetEndpoint.cs` — PUT /api/pets/{id} — Validates → UpdatePetHandler.UpdateAsync → 200 or 404.
- `src/Petit.WebApi/Features/Pets/DeletePetEndpoint.cs` — DELETE /api/pets/{id} → DeletePetHandler.DeleteAsync → 204 or 404.

### Test files (new)
- `tests/Petit.WepApi.Tests/Features/Pets/CreatePetEndpointTests.cs` — WebApplicationFactory-based integration test: sends request, asserts 201 + persisted pet using an in-memory PostgreSQL connection per appsettings for dev or InMemory provider for tests.
- `tests/Petit.WepApi.Tests/Features/Pets/ListPetsHandlerTests.cs` — Handler unit test: uses InMemory EF Core provider, asserts projection results and ownership filtering.

## Order

1. **Add required NuGet packages** to `Petit.WebApi.csproj`: `FluentValidation`, `FluentValidation.DependencyInjectionExtensions`, [`EFCore.Npgsql`](https://www.nuget.org/packages/EFCore.Npgsql), [`Microsoft.AspNetCore.Authentication.JwtBearer`](https://www.nuget.org/packages/Microsoft.AspNetCore.Authentication.JwtBearer)
2. **Configure auth in Program.cs** (before building app): Add Keycloak JWT Bearer authentication middleware — Authority and audience from configuration only, never hardcoded. This is a prerequisite for all subsequent endpoint code.
3. **Add dev connection string to `appsettings.json`**: e.g. `"PetDbConnection": "Host=localhost;Port=5432;Database=pets_local;Username=postgres;Password=postgres"` — used by EF Core setup in step 4.
4. **Configure EF Core** in Program.cs: use the named connection string from `appsettings` (or env var) with `EFCore.Npgsql`, call `AddDbContext<AppDbContext>();`, wire `DbSet<Pet>`. No migration plumbing or seed logic here — just the registration so persistence is live before any handler runs.
5. **Create entity + config**: Write `Pet.cs` and `PetEntityTypeConfiguration.cs`. The entity mirrors the spec's data fields exactly. Configuration goes into its own file per CONSTITUTION.md, NOT Fluent API in OnModelCreating.
6. **Write RequestModels** (DTO/Request): `CreatePetRequest.cs`, `UpdatePetRequest.cs`, `PetResponse.cs`. Simple POCOs, no constructors needed — C# 12 init/record or simple class with properties.
7. **Write Validators per CONSTITUTION.md**: `CreatePetValidator.cs` and `UpdatePetValidator.cs`. FluentValidation classes called explicitly in the endpoint (not via behavior pipeline). Each validator has only the rules that apply to its action (e.g., Update ignores required-on-create fields).
8. **Write handlers first** (data access layer): `CreatePetHandler`, `ListPetsHandler`, `GetPetHandler`, `UpdatePetHandler`, `DeletePetHandler`. Each injects DbContext directly per CONSTITUTION.md — no Repository<T>. OwnerId always comes from the handler parameter (set later by the endpoint from JWT), never from user input.
9. **Write endpoints**: Create, List, Get, Update, Delete. Each follows the strict CONSTITUTION.md pattern: validate → call handler → map result, zero business logic. Register them under a `/api/pets` route group with `[RequireAuthorization]`.
10. **Write integration tests**: One for each command operation using `WebApplicationFactory<Program>`. All tests register an EF Core InMemory DB provider (`optionsBuilder.UseInMemoryDatabase(...)` or via the factory's test scope) to keep them fast, deterministic, and free of external database dependencies. Assert HTTP status codes and persisted data.

## Risks
- (none remaining — package choice confirmed as official `EFCore.Npgsql`; dev connection string goes into `appsettings.json`; weather forecast scaffold is being fully removed in step 2)

## Completion criteria
- All listed files exist and match their stated purpose
- `dotnet build` passes with no errors or warnings related to this feature
- Each endpoint maps correctly: POST /api/pets, GET /api/pets, GET /api/pets/{id}, PUT /api/pets/{id}, DELETE /api/pets/{id}
- Auth via Keycloak JWT works — unauthenticated requests return 401
- Owner filtering is enforced in every query (ownership by OwnerId from JWT, never user-supplied)
- FluentValidation runs in the endpoint before any handler call; invalid input returns HTTP 400 with structured details
- `dotnet test` passes
