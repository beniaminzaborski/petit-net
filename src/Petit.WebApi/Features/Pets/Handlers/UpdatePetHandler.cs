using Microsoft.EntityFrameworkCore;
using Petit.WebApi.Data;
using Petit.WebApi.RequestModels;

namespace Petit.WebApi.Features.Pets.Handlers;

public static class UpdatePetHandler
{
    public static async Task<(int StatusCode, PetResponse? Data)> UpdateAsync(
        PetDbContext context,
        Guid id,
        UpdatePetRequest request,
        string userId)
    {
        var pet = await context.Pets
            .FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == userId);

        if (pet is null)
        {
            return (404, null);
        }

        pet.Name = request.Name;
        pet.Type = request.Type;
        pet.Gender = request.Gender;
        pet.Breed = request.Breed;
        pet.Birthday = request.Birthday;
        pet.Weight = request.Weight;
        pet.Neutered = request.Neutered;
        pet.Description = request.Description;
        pet.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        var response = new PetResponse(
            pet.Id,
            pet.Name,
            pet.Type,
            pet.Gender,
            pet.Breed,
            pet.Birthday,
            pet.Weight,
            pet.Neutered,
            pet.Description,
            pet.OwnerId,
            pet.CreatedAt,
            pet.UpdatedAt);

        return (200, response);
    }
}
