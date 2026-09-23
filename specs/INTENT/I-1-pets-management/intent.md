---
id: I-1-pets-management
status: approved
---

# Intent: Pet CRUD — full resource management

## Ask
REST API endpoints for managing pets: create, list, get by ID, update, and delete.

## Why
Core resource management functionality for a pet-tracking backend. Without this there is no way to store or retrieve any pet data.

A pet has:
- name (required)
- type — e.g. dog, cat (required)
- gender (required)
- breed (optional)
- birthday (optional)
- weight (optional)
- neutered (optional)
- description (optional)
- owner — linked to the requesting user via JWT authentication

## Constraints
- Auth via Keycloak JWT; users can only see and modify their own pets
- PostgreSQL with EF Core 10
- .NET 10, Minimal API (per CONSTITUTION.md)

## Open Questions
- Should list include pagination? If yes, what defaults (page size)?
- Should delete be hard-delete or soft-delete (with `DeletedAt`)?
- Is the owner immutable after creation, or can it change via update?
- What are the allowed values for pet type and gender?
``