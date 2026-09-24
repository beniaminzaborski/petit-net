---
id: I-2-food-management
status: approved
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
- No pagination on list — return all matching food items
- Delete is hard-delete
- Owner is immutable after creation
- Allowed values for `type`: Dry, Wet, Raw, Treats, Other
- Allowed values for `petType`: Dog, Cat, Other
- Free-text search matches `name` or `producer`, case-insensitive, partial match
- `caloriesPer100g` and `servingSize` must be between 0 and 10000
