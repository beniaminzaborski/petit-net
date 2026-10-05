---
id: I-2-food-management
status: draft
---

# Plan: Food CRUD — full resource management

## Files

### New files (under `src/Petit.WebApi/`)

| File | Purpose |
|------|---------|
| `Data/Food.cs` (new) | Entity with all Food fields + `OwnerId`, `CreatedAt`, `UpdatedAt` |
| `Data/FoodEntityTypeConfiguration.cs` (new) | EF Core config for Food (key, required props, indexes) |
| `Features/Food/FoodResponse.cs` (new) | Record matching the response shape of R2/R3/R4 |
| `RequestModels/CreateFoodRequest.cs` (new) | Request model for create |
| `RequestModels/UpdateFoodRequest.cs` (new) | Request model for update |
| `Features/Food/Validators/CreateFoodValidator.cs` (new) | FluentValidation for create |
| `Features/Food/Validators/UpdateFoodValidator.cs` (new) | FluentValidation for update |
| `Features/Food/Handlers/CreateFoodHandler.cs` (new) | Business logic: insert Food, return 201 + response |
| `Features/Food/Handlers/ListFoodHandler.cs` (new) | Business logic: query with search/filter, return all matches |
| `Features/Food/Handlers/GetFoodHandler.cs` (new) | Business logic: find by ID + owner, return 200 or 404 |
| `Features/Food/Handlers/UpdateFoodHandler.cs` (new) | Business logic: find + update fields, return 200 or 404 |
| `Features/Food/Handlers/DeleteFoodHandler.cs` (new) | Business logic: find + hard-delete, return 204 or 404 |
| `Features/Food/CreateFoodEndpoint.cs` (new) | `POST /api/foods` — map request → validate → call handler |
| `Features/Food/ListFoodEndpoint.cs` (new) | `GET /api/foods` — query params for search/filter → call handler |
| `Features/Food/GetFoodEndpoint.cs` (new) | `GET /api/foods/{id:guid}` — lookup by ID → call handler |
| `Features/Food/UpdateFoodEndpoint.cs` (new) | `PUT /api/foods/{id:guid}` — update → call handler |
| `Features/Food/DeleteFoodEndpoint.cs` (new) | `DELETE /api/foods/{id:guid}` — delete → call handler |

### Modified files

| File | Purpose |
|------|---------|
| `Data/PetDbContext.cs` (modified) | Add `DbSet<Food> Foods` property and `ApplyConfiguration(new FoodEntityTypeConfiguration())` in `OnModelCreating` |
| `Program.cs` (modified) | Add `app.MapCreateFood()`, `app.MapListFood()`, `app.MapGetFood()`, `app.MapUpdateFood()`, `app.MapDeleteFood()` calls |

### New test files (under `tests/Petit.WepApi.Tests/`)

| File | Purpose |
|------|---------|
| `Features/Food/CreateFoodEndpointTests.cs` (new) | Integration tests: 201 on valid create, 400 on validation failures |
| `Features/Food/ListFoodEndpointTests.cs` (new) | Integration tests: 200 with search/filter results, empty list when no match |
| `Features/Food/GetFoodEndpointTests.cs` (new) | Integration tests: 200 for owned food, 404 for missing/unowned |
| `Features/Food/UpdateFoodEndpointTests.cs` (new) | Integration tests: 200 on valid update, 404 for missing/unowned, owner immutability |
| `Features/Food/DeleteFoodEndpointTests.cs` (new) | Integration tests: 204 on delete, 404 for missing/unowned |

## Order

1. **Data layer** — Create `Food.cs` entity and `FoodEntityTypeConfiguration.cs`, then modify `PetDbContext.cs` to register the new entity. This is the foundation everything else depends on.

2. **Request models** — Create `CreateFoodRequest.cs` and `UpdateFoodRequest.cs` in `RequestModels/`. These are plain records with no dependencies.

3. **Validators** — Create `CreateFoodValidator.cs` and `UpdateFoodValidator.cs` in `Features/Food/Validators/`. Depend on request models from step 2.

4. **Response model** — Create `FoodResponse.cs` in `Features/Food/`. Plain record, no dependencies beyond types already defined.

5. **Handlers** — Create the five handler classes in `Features/Food/Handlers/`:
   - `CreateFoodHandler.cs` (depends on Food entity + request models)
   - `ListFoodHandler.cs` (depends on Food entity)
   - `GetFoodHandler.cs` (depends on Food entity)
   - `UpdateFoodHandler.cs` (depends on Food entity + request models)
   - `DeleteFoodHandler.cs` (depends on Food entity)

6. **Endpoints** — Create the five endpoint files in `Features/Food/`:
   - `CreateFoodEndpoint.cs`
   - `ListFoodEndpoint.cs`
   - `GetFoodEndpoint.cs`
   - `UpdateFoodEndpoint.cs`
   - `DeleteFoodEndpoint.cs`
   Each depends on handlers (step 5), validators (step 3), request models (step 2), and response model (step 4).

7. **Program.cs wiring** — Add the five `app.Map*Food()` calls to `Program.cs`. Depends on all endpoint files from step 6.

8. **Tests** — Create the five test classes in `tests/Petit.WepApi.Tests/Features/Food/`, following the same `[Collection("Pets")]` pattern (rename to `[Collection("Foods")]`) and `IClassFixture<TestWebApplicationFactory>` pattern used for Pets tests. Depends on all production code from steps 1–7.

## Risks
- **InMemory database with Food entity**: The test factory uses EF Core InMemory provider. Need to verify that the `FoodEntityTypeConfiguration` (especially indexes and property constraints) works correctly with InMemory — some EF Core configurations behave differently in InMemory vs Npgsql. If issues arise, may need to adjust configuration or add conditional logic in tests.
- **Handler return type consistency**: The spec says handlers return `(int StatusCode, FoodResponse? Data)` tuples, but `DeleteFoodHandler` returns just `int`. Need to ensure endpoint code handles this correctly (no `.Data` access for delete).
- **Search with `EF.Functions.Like` in InMemory**: EF Core's InMemory provider does not support `EF.Functions.Like()`. The list tests will need to either skip the search portion or use a different approach (e.g., filter in-memory after loading). This is a known limitation — may need conditional test logic.
- **Owner immutability testing**: Need to verify that update endpoint correctly rejects attempts to change `OwnerId` — this should be enforced by not exposing `OwnerId` in the request model and only setting it at creation time.

## Completion criteria
- All 17 new files exist and match their stated purpose
- `PetDbContext.cs` includes `DbSet<Food> Foods` and applies `FoodEntityTypeConfiguration`
- `Program.cs` wires all five food endpoints
- `dotnet build` passes with no warnings
- `dotnet test` passes — all Food integration tests (15+ tests across 5 classes) pass consistently
- `[Collection("Foods")]` applied to all Food test classes to serialize execution (same pattern as Pets)
- No FluentValidation, AutoMapper, MediatR, or other forbidden patterns introduced
- All handlers are `static` classes with `async` methods returning tuples (matching Pet handler convention)
- All endpoints use `[FromBody]`, `[FromServices]`, `[FromQuery]` attributes and `.RequireAuthorization()` (matching Pet endpoint convention)
