using Microsoft.EntityFrameworkCore;
using Petit.WebApi.Data;

namespace Petit.WebApi.Features.Food.Handlers;

public static class DeleteFoodHandler
{
    public static async Task<int> DeleteAsync(
        PetitDbContext context,
        Guid id,
        string userId)
    {
        var food = await context.Foods
            .FirstOrDefaultAsync(f => f.Id == id && f.OwnerId == userId);

        if (food is null)
        {
            return 404;
        }

        context.Foods.Remove(food);
        await context.SaveChangesAsync();

        return 204;
    }
}
