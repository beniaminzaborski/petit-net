using Microsoft.EntityFrameworkCore;
using Petit.WebApi.Data;
using Petit.WebApi.RequestModels;

namespace Petit.WebApi.Features.Pets.Handlers;

public static class CreatePetHandler
{
    public static async Task<(int StatusCode, PetResponse Data)> CreateAsync(
        PetDbContext context,
        CreatePetRequest request,
        string userId)
    {
        var pet = new Pet
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Type = request.Type,
            Gender = request.Gender,
            Breed = request.Breed,
            Birthday = request.Birthday,
            Weight = request.Weight,
            Neutered = request.Neutered,
            Description = request.Description,
            OwnerId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        context.Pets.Add(pet);
        await context.SaveChangesAsync();

        var response = MapToResponse(pet);
        return (201, response);
    }

    private static PetResponse MapToResponse(Pet pet) => new(
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
}
