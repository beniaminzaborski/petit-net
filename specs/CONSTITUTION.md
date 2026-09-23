# Constitution — MyApp Backend

These rules are binding for all code generated in this repository. Any
deviation must be flagged explicitly (e.g. in a spec.md "Flagged concerns"
section) rather than applied silently.

## Stack
- .NET 10, C# 13
- Minimal API — NO controllers, NO [ApiController]
- EF Core 10 + Npgsql (PostgreSQL)
- Authentication: Keycloak as IdP, JWT Bearer validated via .NET middleware
- Backend is REST API only — no Razor, no SSR, no views

## Architecture — principles
- Feature-folder (vertical slice) structure, NOT layer-folder structure
  (i.e. not top-level Controllers/Services/Repositories)
- One endpoint = one file in `Features/{Feature}/{Action}Endpoint.cs`
- Endpoint = map HTTP request -> call handler -> map result. No business
  logic inside the endpoint itself.
- Business logic lives in handler/service classes next to the endpoint,
  in the same feature folder
- Do NOT introduce: MediatR, generic Repository<T>, Unit of Work as a
  separate abstraction, or full CQRS infrastructure — these are
  over-architecture at this project's scale
- Abstraction over EF Core: yes, but minimal — a feature-specific interface
  (e.g. `IClaimsRepository`) with 3-5 methods that are actually used. NOT a
  generic CRUD repository.
- Inject DbContext directly where it's simple and local (e.g. in simple
  query handlers) — add an abstraction only where it earns testability or
  where the logic is non-trivial

## EF Core — rules
- Migrations: `dotnet ef migrations add` per feature/change, descriptive name
- Entity configuration via `IEntityTypeConfiguration<T>`, NOT Fluent API
  scattered across OnModelCreating
- No lazy loading (explicit `.Include()` or projections via `.Select()`)
- Read queries: project directly to a DTO (`.Select()`) — don't load full
  entities where they're not needed

## Auth — Keycloak JWT
- Middleware: `AddAuthentication().AddJwtBearer()` with Authority pointing
  to the Keycloak realm
- Authorization via `RequireAuthorization()` at the route level, with a
  policy per permission where needed
- Claims from the token are mapped onto `ClaimsPrincipal` — no custom
  session cache
- Secrets (Keycloak URL, client id) live in configuration (appsettings +
  env vars), NEVER hardcoded

## Clean code — what this means here
- Methods under 30 lines, one responsibility per class
- Explicit names (`GetClaimStatusAsync`, not `Process` or `Handle`)
- Input validation: FluentValidation, called explicitly in the endpoint
  before business logic runs
- Exceptions are for truly exceptional situations only — validation/business
  errors are returned as a result/response, not thrown
- Unit tests for business logic, integration tests (WebApplicationFactory)
  for endpoints

## Forbidden patterns
- MVC controllers
- Generic Repository<T> / Unit of Work as a separate layer
- MediatR / CQRS with full infrastructure (Commands/Queries/Handlers as a
  separate assembly)
- AutoMapper (mapping is explicit, so it's clear what's happening)
- Static services / singletons with mutable state