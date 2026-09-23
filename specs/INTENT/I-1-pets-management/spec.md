---
id: I-1-pets-management
status: approved
---

# Spec: Pet CRUD — full resource management

## Requirements
- R1: Create a pet with name (required), type (required, e.g. dog/cat), gender (required), and optional breed, birthday, weight, neutered flag, description; owner is resolved from the JWT (not provided by the client).
- R2: List all pets belonging to the requesting user, with pagination — page number (default 0) and page size (default 20); supports an optional `type` filter.
- R3: Get a single pet by ID; returns 404 if the pet does not exist or does not belong to the requesting user.
- R4: Update all mutable properties of a pet owned by the requesting user (name, type, gender, breed, birthday, weight, neutered, description). Returns 404 if the pet does not belong to the user.
- R5: Delete a pet belonging to the requesting user; returns 404 if the pet does not exist or does not belong to the user.
- R6: All endpoints require Keycloak JWT authentication (RequireAuthorization).
- R7: Input validation via FluentValidation, called explicitly in the endpoint before any business logic runs — validation errors return HTTP 400 with a structured problem details response.

## Design

### Entity & Database
- `Pet` entity in `Features/Pets/Entities/Pet.cs`
  - Properties: Id (int/guid), Name (string, required), Type (string), Gender (string), Breed (string? nullable), Birthday (DateTime?), Weight (decimal?), Neutered (bool?), Description (string? nullable), OwnerId (string from JWT), CreatedAt, UpdatedAt
- `PetEntityTypeConfiguration` in `Features/Pets/Configurations/PetEntityTypeConfiguration.cs` implementing `IEntityTypeConfiguration<Pet>`
  - Configures required fields, optional fields, indexes on `OwnerId` and `Type`
- DbContext extends the existing context; `Pet` added as a DbSet via `AddConfiguration` or model builder in `OnModelCreating` (which already accepts configurations per CONSTITUTION.md)

### Feature Folder Structure — `Features/Pets/`
```
Features/Pets/
  Entities/
    Pet.cs
  Configurations/
    PetEntityTypeConfiguration.cs
  RequestModels/
    CreatePetRequest.cs
    UpdatePetRequest.cs
    PetResponse.cs
  Validators/
    CreatePetValidator.cs
    UpdatePetValidator.cs
  Handlers/
    CreatePetHandler.cs
    ListPetsHandler.cs
    GetPetHandler.cs
    UpdatePetHandler.cs
    DeletePetHandler.cs
  CreatePetEndpoint.cs
  ListPetsEndpoint.cs
  GetPetEndpoint.cs
  UpdatePetEndpoint.cs
  DeletePetEndpoint.cs
```

### Endpoints (one file each, in `Features/Pets/`)

Each endpoint follows the CONSTITUTION.md pattern: map HTTP request → validate with FluentValidation → call handler → map result. No business logic in the endpoint itself.

**POST /api/pets — CreatePetEndpoint.cs**
- Validates request body with `CreatePetValidator`
- Calls `CreatePetHandler.CreateAsync(request, userId)` where `userId` comes from `ClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier)`
- Returns 201 with `PetResponseDto` (created resource location in `Location` header)

**GET /api/pets — ListPetsEndpoint.cs**
- Optional query parameters: `page` (default 0), `pageSize` (default 20), `type` (optional filter)
- Calls `ListPetsHandler.ListAsync(page, pageSize, typeFilter, userId)` — handler queries `DbSet<Pet>` directly with `.Where(p => p.OwnerId == userId).Select(...)` projection to `PetResponseDto[]`
- Returns 200 with `{ data: PetResponseDto[], page, pageSize, totalItems }`

**GET /api/pets/{id} — GetPetEndpoint.cs**
- Calls `GetPetHandler.GetByIdAsync(id, userId)` — queries directly via DbContext `.FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == userId)`
- Returns 200 with `PetResponseDto` or 404 if not found/null

**PUT /api/pets/{id} — UpdatePetEndpoint.cs**
- Validates request body with `UpdatePetValidator`
- Calls `UpdatePetHandler.UpdateAsync(id, request, userId)` — fetches via DbContext, checks ownership, updates owned properties, saves
- Returns 200 with updated `PetResponseDto` or 404 if not found

**DELETE /api/pets/{id} — DeletePetEndpoint.cs**
- Calls `DeletePetHandler.DeleteAsync(id, userId)` — fetches via DbContext, checks ownership, removes, saves
- Returns 204 No Content or 404 if not found

### Handlers — design notes
- CRUD that only reads (Create, List, Get) injects `DbContext` directly as it is simple and local.
- Update and Delete also inject `DbContext` directly for the same reason per CONSTITUTION.md: "Inject DbContext directly where it's simple and local".
- No abstraction over EF Core beyond what's needed — no Repository<T>.
- All handlers operate only on pets belonging to the requesting user (`OwnerId == userId` from JWT).

### DTOs
- `PetResponseDto`: Id, Name, Type, Gender, Breed?, Birthday?, Weight?, Neutered?, Description?
- Request DTOs follow C# naming conventions (e.g. `CreatePetRequest`, not with trailing `Dto`)
- Mapping is explicit per CONSTITUTION.md — no AutoMapper

### Auth
- All endpoints decorated with `[RequireAuthorization]` via Minimal API `requirement:` syntax
- User identity from JWT claims: `context.User.FindFirstValue(ClaimTypes.NameIdentifier)` for `OwnerId` filtering
- Keycloak JWT validation already configured in Program.cs per CONSTITUTION.md (no changes needed to auth setup)

### Error Responses (non-validation)
- 404 Not Found for entities missing or not owned by the user — returned as application/problem+json
- No exceptions for business errors — results are returned explicitly

## Constraints
- Auth via Keycloak JWT; users can only see and modify their own pets
- PostgreSQL with EF Core 10
- .NET 10, Minimal API (per CONSTITUTION.md)
- One endpoint = one file in `Features/{Feature}/{Action}Endpoint.cs` (CONSTITUTION.md)
- FluentValidation called explicitly in the endpoint before business logic (CONSTITUTION.md)

## ⚠️ Flagged concerns
1. **Pagination defaults not specified in intent**: Defaulting to page 0, pageSize 20, offset-based. Flagged because CONSTITUTION.md says "Do NOT invent requirements" — the intent has an open question on this. Recommend confirming before planning.
  
2. **Delete strategy unclear in intent** (hard vs soft delete): Assuming hard-delete for simplicity given CRVU scale and no existing soft-delete pattern. Can be revisited if needed later.

3. **Owner immutability not addressed in intent**: Assuming owner is set at creation time based on JWT and becomes immutable after — this simplifies the update endpoint and avoids cross-user ownership issues. Flagged because CONSTITUTION.md says to flag decisions when requirements are ambiguous.

4. **Type/Gender allowed values not specified**: Using `string` for Type and Gender with no enum or constraint in the database. This is the simplest approach that avoids inventing rules, but a future refinement could add validation constraints (e.g., CHECK constraint or domain enum). Flagged as an open question for the planner to confirm.

5. **No error handling strategy beyond 404**: The intent only mentions "update" and "delete". If business errors like concurrent updates need handling (optimistic concurrency), that's not specified but should be considered during planning.
