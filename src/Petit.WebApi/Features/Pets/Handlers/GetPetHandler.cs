using Microsoft.EntityFrameworkCore;
using Petit.WebApi.Data;

namespace Petit.WebApi.Features.Pets.Handlers;

public static class GetPetHandler
{
    public static async Task<(int StatusCode, PetResponse? Data)> GetByIdAsync(
        PetDbContext context,
        Guid id,
        string userId)
    {
        var pet = await context.Pets
            .Where(p => p.Id == id && p.OwnerId == userId)
            .Select(p => new PetResponse(
                p.Id,
                p.Name,
                p.Type,
                p.Gender,
                p.Breed,
                p.Birthday,
                p.Weight,
                p.Neutered,
                p.Description,
                p.OwnerId,
                p.CreatedAt,
                p.UpdatedAt))
            .FirstOrDefaultAsync();

        if (pet is null)
        {
            return (404, null);
        }

        return (200, pet);
    }
}
