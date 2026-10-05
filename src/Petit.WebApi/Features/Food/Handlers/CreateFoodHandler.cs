using Microsoft.EntityFrameworkCore;
using Petit.WebApi.Data;
using Petit.WebApi.RequestModels;

namespace Petit.WebApi.Features.Food.Handlers;

public static class CreateFoodHandler
{
    public static async Task<(int StatusCode, FoodResponse Data)> CreateAsync(
        PetitDbContext context,
        CreateFoodRequest request,
        string userId)
    {
        var food = new Petit.WebApi.Data.Food
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Producer = request.Producer,
            Type = request.Type,
            PetType = request.PetType,
            CaloriesPer100g = request.CaloriesPer100g,
            ServingSize = request.ServingSize,
            SubCategory = request.SubCategory,
            Description = request.Description,
            OwnerId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        context.Foods.Add(food);
        await context.SaveChangesAsync();

        var response = MapToResponse(food);
        return (201, response);
    }

    private static FoodResponse MapToResponse(Petit.WebApi.Data.Food food) => new(
        food.Id,
        food.Name,
        food.Producer,
        food.Type,
        food.PetType,
        food.CaloriesPer100g,
        food.ServingSize,
        food.SubCategory,
        food.Description,
        food.OwnerId,
        food.CreatedAt,
        food.UpdatedAt);
}
