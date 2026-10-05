using Microsoft.EntityFrameworkCore;
using Petit.WebApi.Data;

namespace Petit.WebApi.Features.Food.Handlers;

public static class ListFoodsHandler
{
    public static async Task<(int StatusCode, FoodResponse[] Data, int TotalItems)> ListAsync(
        PetitDbContext context,
        string userId,
        int page = 0,
        int pageSize = 20,
        string? typeFilter = null)
    {
        var query = context.Foods.Where(f => f.OwnerId == userId);

        if (!string.IsNullOrWhiteSpace(typeFilter))
        {
            query = query.Where(f => f.Type == typeFilter);
        }

        var totalItems = await query.CountAsync();

        var foods = await query
            .OrderBy(f => f.Name)
            .Skip(page * pageSize)
            .Take(pageSize)
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
            .ToArrayAsync();

        return (200, foods, totalItems);
    }
}
