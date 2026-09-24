---
id: I-2-food-management
status: draft
---

# Intent: Food CRUD — full resource management

## Ask
REST API endpoints for managing a catalog of pet food products — create a food item, list food items, get a single food item by ID, update a food item, delete a food item.

## Why
Core resource management functionality for a pet-food catalog — users maintain their own list of food products to reference when tracking what their pets eat. Without this there is no way to store or retrieve any food data.

A food item has:
- name (required)
- producer (required)
- type — e.g. dry, wet, raw, treats, other (required)
- petType — e.g. dog, cat, other (optional)
- caloriesPer100g (required)
- servingSize (required)
- subCategory (optional)
- description (optional)
- owner — linked to the requesting user via JWT authentication

Listing also supports free-text search (matching name or producer) and optional filtering by food type and pet type.

## Constraints
- Auth via Keycloak JWT; users can only see and modify their own food items
- PostgreSQL with EF Core 10
- .NET 10, Minimal API (per CONSTITUTION.md)

## Open Questions
- Should list include pagination? If yes, what defaults (page size)?
- Should delete be hard-delete or soft-delete (with `DeletedAt`)?
- Is the owner immutable after creation, or can it change via update?
- What are the allowed values for food type and pet type enums?
- How should free-text search behave — case-insensitive, partial match, word boundaries?
- What are valid ranges for numeric fields (caloriesPer100g, servingSize)?
