using Microsoft.EntityFrameworkCore;
using Petit.WebApi.Data;
using Petit.WebApi.RequestModels;

namespace Petit.WebApi.Features.Food.Handlers;

public static class UpdateFoodHandler
{
    public static async Task<(int StatusCode, FoodResponse? Data)> UpdateAsync(
        PetitDbContext context,
        Guid id,
        UpdateFoodRequest request,
        string userId)
    {
        var food = await context.Foods
            .FirstOrDefaultAsync(f => f.Id == id && f.OwnerId == userId);

        if (food is null)
        {
            return (404, null);
        }

        food.Name = request.Name;
        food.Producer = request.Producer;
        food.Type = request.Type;
        food.PetType = request.PetType;
        food.CaloriesPer100g = request.CaloriesPer100g;
        food.ServingSize = request.ServingSize;
        food.SubCategory = request.SubCategory;
        food.Description = request.Description;
        food.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        var response = new FoodResponse(
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

        return (200, response);
    }
}
