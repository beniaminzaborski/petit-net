using Microsoft.EntityFrameworkCore;
using Petit.WebApi.Data;

namespace Petit.WebApi.Features.Food.Handlers;

public static class GetFoodHandler
{
    public static async Task<(int StatusCode, FoodResponse? Data)> GetByIdAsync(
        PetitDbContext context,
        Guid id,
        string userId)
    {
        var food = await context.Foods
            .Where(f => f.Id == id && f.OwnerId == userId)
            .Select(f => new FoodResponse(
                f.Id,
                f.Name,
                f.Producer,
                f.Type,
                f.PetType,
                f.CaloriesPer100g,
                f.ServingSize,
                f.SubCategory,
                f.Description,
                f.OwnerId,
                f.CreatedAt,
                f.UpdatedAt))
            .FirstOrDefaultAsync();

        if (food is null)
        {
            return (404, null);
        }

        return (200, food);
    }
}
