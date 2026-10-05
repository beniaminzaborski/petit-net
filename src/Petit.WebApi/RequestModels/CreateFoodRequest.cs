namespace Petit.WebApi.RequestModels;

public sealed record CreateFoodRequest(
    string Name,
    string Producer,
    string Type,
    string? PetType,
    int CaloriesPer100g,
    int ServingSize,
    string? SubCategory,
    string? Description);
