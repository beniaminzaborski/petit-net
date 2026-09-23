using Microsoft.EntityFrameworkCore;
using Petit.WebApi.Data;

namespace Petit.WebApi.Features.Pets.Handlers;

public static class DeletePetHandler
{
    public static async Task<int> DeleteAsync(
        PetDbContext context,
        Guid id,
        string userId)
    {
        var pet = await context.Pets
            .FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == userId);

        if (pet is null)
        {
            return 404;
        }

        context.Pets.Remove(pet);
        await context.SaveChangesAsync();

        return 204;
    }
}
