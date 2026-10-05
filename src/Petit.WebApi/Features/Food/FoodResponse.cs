namespace Petit.WebApi.Features.Food;

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
