---
id: I-2-food-management
status: approved
---

# Spec: Food CRUD — full resource management

## Requirements
- R1: Users can create a food item with name (required), producer (required), type (required, one of Dry/Wet/Raw/Treats/Other), petType (optional, one of Dog/Cat/Other), caloriesPer100g (required, 0–10000), servingSize (required, 0–10000), subCategory (optional), description (optional).
- R2: Users can list all their food items with optional free-text search on name or producer (case-insensitive partial match) and optional filtering by type and petType. No pagination — return all matching items.
- R3: Users can retrieve a single food item by ID, but only if it belongs to them.
- R4: Users can update their own food items; owner is immutable after creation.
- R5: Users can hard-delete their own food items.
- R6: All endpoints require Keycloak JWT authentication; users can only see/modify their own food items.

## Design

### New files (under `Features/Food/`)

| File | Purpose |
|------|---------|
| `Food.cs` | Entity with all Food fields + `OwnerId`, `CreatedAt`, `UpdatedAt` |
| `FoodEntityTypeConfiguration.cs` | EF Core config for Food (key, required props, indexes) |
| `FoodResponse.cs` | Record matching the response shape of R2/R3/R4 |
| `CreateFoodRequest.cs` | Request model for create (in `RequestModels/`) |
| `UpdateFoodRequest.cs` | Request model for update (in `RequestModels/`) |
| `Validators/CreateFoodValidator.cs` | FluentValidation for create |
| `Validators/UpdateFoodValidator.cs` | FluentValidation for update |
| `Handlers/CreateFoodHandler.cs` | Business logic: insert Food, return 201 + response |
| `Handlers/ListFoodHandler.cs` | Business logic: query with search/filter, return all matches |
| `Handlers/GetFoodHandler.cs` | Business logic: find by ID + owner, return 200 or 404 |
| `Handlers/UpdateFoodHandler.cs` | Business logic: find + update fields, return 200 or 404 |
| `Handlers/DeleteFoodHandler.cs` | Business logic: find + hard-delete, return 204 or 404 |
| `CreateFoodEndpoint.cs` | `POST /api/foods` — map request → validate → call handler |
| `ListFoodEndpoint.cs` | `GET /api/foods` — query params for search/filter → call handler |
| `GetFoodEndpoint.cs` | `GET /api/foods/{id:guid}` — lookup by ID → call handler |
| `UpdateFoodEndpoint.cs` | `PUT /api/foods/{id:guid}` — update → call handler |
| `DeleteFoodEndpoint.cs` | `DELETE /api/foods/{id:guid}` — delete → call handler |

### Entity design (Food)

```csharp
public class Food
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Producer { get; set; } = "";
    public string Type { get; set; } = "";
    public string? PetType { get; set; }
    public int CaloriesPer100g { get; set; }
    public int ServingSize { get; set; }
    public string? SubCategory { get; set; }
    public string? Description { get; set; }
    public string OwnerId { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

- `OwnerId` is set at creation time from the JWT claim and never updated (R4).
- `CreatedAt` / `UpdatedAt` follow the same pattern as Pet.

### Entity configuration (`FoodEntityTypeConfiguration`)

- Primary key: `Id` (ValueGeneratedOnAdd)
- Required properties with `HasMaxLength`: Name (100), Producer (100), Type (50), OwnerId (128)
- Optional properties with `HasMaxLength`: SubCategory (100), Description (1000)
- Index on `OwnerId` (same as Pet)
- Index on `{Name, Producer}` to support the free-text search efficiently (same pattern as Pet's `{Name, Type}` index)
- No validation constraints at the EF level for type/petType enums — those are enforced in validators

### Response shape (`FoodResponse`)

```csharp
public sealed record FoodResponse(
    Guid Id,
    string Name,
    string Producer,
    string Type,
    string? PetType,
    int CaloriesPer100g,
    int ServingSize,
    string? SubCategory,
    string? Description,
    string OwnerId,
    DateTime CreatedAt,
    DateTime UpdatedAt);
```

### Request models (in `RequestModels/`)

`CreateFoodRequest`:
```csharp
public sealed record CreateFoodRequest(
    string Name,
    string Producer,
    string Type,
    string? PetType,
    int CaloriesPer100g,
    int ServingSize,
    string? SubCategory,
    string? Description);
```

`UpdateFoodRequest`:
```csharp
public sealed record UpdateFoodRequest(
    string Name,
    string Producer,
    string Type,
    string? PetType,
    int CaloriesPer100g,
    int ServingSize,
    string? SubCategory,
    string? Description);
```

### Validators

`CreateFoodValidator`:
- `Name` not empty → "Name is required."
- `Producer` not empty → "Producer is required."
- `Type` not empty + one of Dry/Wet/Raw/Treats/Other → "Type must be one of: Dry, Wet, Raw, Treats, Other."
- `CaloriesPer100g` in range 0–10000 → "CaloriesPer100g must be between 0 and 10000."
- `ServingSize` in range 0–10000 → "ServingSize must be between 0 and 10000."
- `PetType` if provided, must be one of Dog/Cat/Other

`UpdateFoodValidator`: Same rules as CreateFoodValidator.

### Handler patterns (mirroring Pets)

Each handler is a `static` class with an `async` method returning a tuple `(int StatusCode, FoodResponse? Data)` (or `int` for Delete). They receive `PetDbContext` directly (per CONSTITUTION.md: inject DbContext where simple and local).

- **CreateFoodHandler**: Creates Food entity with `OwnerId` from the userId parameter, calls `SaveChangesAsync`, maps to response. Returns `(201, response)`.
- **ListFoodHandler**: Builds query filtered by `OwnerId == userId`, applies optional `typeFilter` and `petTypeFilter`, then applies free-text search via `.Where(f => EF.Functions.Like(f.Name, $"%{search}%") || EF.Functions.Like(f.Producer, $"%{search}%"))`. Projects to `FoodResponse[]` via `.Select()`. Returns `(200, data)`.
- **GetFoodHandler**: `.FirstOrDefaultAsync(f => f.Id == id && f.OwnerId == userId)` projected to response. Returns `(200, response)` or `(404, null)`.
- **UpdateFoodHandler**: Finds by `Id + OwnerId`, updates all mutable fields (not OwnerId), sets `UpdatedAt = DateTime.UtcNow`, saves. Returns `(200, response)` or `(404, null)`.
- **DeleteFoodHandler**: Finds by `Id + OwnerId`, calls `Remove`, saves. Returns `204` or `404`.

### Endpoint patterns (mirroring Pets)

Each endpoint follows the same structure as the Pet endpoints:

```csharp
app.MapPost("/api/foods", async (
        [FromBody] CreateFoodRequest request,
        [FromServices] IValidator<CreateFoodRequest> validator,
        [FromServices] PetDbContext context,
        HttpContext httpContext) => { ... })
    .RequireAuthorization();
```

- All endpoints use `httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)` for the owner.
- Validation errors return `400` with `{ errors: [{ field, message }] }`.
- Not-found returns `404` with `{ error: "Food not found or does not belong to the user." }`.

### List endpoint — no pagination (R2)

The list endpoint differs from Pets in that it has **no** `page`/`pageSize` parameters. It returns all matching items:

```csharp
app.MapGet("/api/foods", async (
        HttpContext httpContext,
        [FromQuery] string? search = null,
        [FromQuery] string? typeFilter = null,
        [FromQuery] string? petTypeFilter = null,
        [FromServices] PetDbContext context) => { ... })
    .RequireAuthorization();
```

The handler returns `(200, FoodResponse[])` — no `totalItems` since there's no pagination.

### DbContext changes

Add to `PetDbContext`:
```csharp
public DbSet<Food> Foods => Set<Food>();
```

Update `OnModelCreating`:
```csharp
modelBuilder.ApplyConfiguration(new FoodEntityTypeConfiguration());
```

### Program.cs changes

Add the Food endpoint registrations:
```csharp
app.MapCreateFood();
app.MapListFood();
app.MapGetFood();
app.MapUpdateFood();
app.MapDeleteFood();
```

## Constraints
- Auth via Keycloak JWT; users can only see and modify their own food items (R6)
- PostgreSQL with EF Core 10
- .NET 10, Minimal API (per CONSTITUTION.md)
- No pagination on list — return all matching food items (R2)
- Delete is hard-delete (R5)
- Owner is immutable after creation (R4)
- Allowed values for `type`: Dry, Wet, Raw, Treats, Other
- Allowed values for `petType`: Dog, Cat, Other
- Free-text search matches `name` or `producer`, case-insensitive, partial match
- `caloriesPer100g` and `servingSize` must be between 0 and 10000

## ⚠️ Flagged concerns
1. **Shared DbContext**: Food entities live in the existing `PetDbContext`. This is consistent with the current architecture (one DB context for the whole app), but if the project grows to have multiple bounded contexts, this may need splitting. Noted as a future consideration, not a blocker.
2. **Type validation at EF level**: The intent specifies allowed values for `type` and `petType`. Currently these are only validated in FluentValidation. Should we also add a CHECK constraint in the EF Core configuration to enforce valid values at the database level? This would prevent invalid data from bypassing validators (e.g., direct DB writes). Flagged for decision before implementation.
3. **Search performance**: The free-text search uses `EF.Functions.Like()` with `%` wildcards, which translates to `ILIKE` in PostgreSQL. For large datasets this could be slow. The intent says "no pagination" and the Pets feature doesn't have full-text search either, so this is acceptable for now. Flagged as a potential optimization later.
4. **No spec.md existed yet**: This is the first time writing a spec.md for this feature — no prior spec to check against.
