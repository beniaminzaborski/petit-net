using Microsoft.EntityFrameworkCore;
using Petit.WebApi.Data;

namespace Petit.WebApi.Features.Pets.Handlers;

public static class ListPetsHandler
{
    public static async Task<(int StatusCode, PetResponse[] Data, int TotalItems)> ListAsync(
        PetDbContext context,
        string userId,
        int page = 0,
        int pageSize = 20,
        string? typeFilter = null)
    {
        var query = context.Pets.Where(p => p.OwnerId == userId);

        if (!string.IsNullOrWhiteSpace(typeFilter))
        {
            query = query.Where(p => p.Type == typeFilter);
        }

        var totalItems = await query.CountAsync();

        var pets = await query
            .OrderBy(p => p.Name)
            .Skip(page * pageSize)
            .Take(pageSize)
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
            .ToArrayAsync();

        return (200, pets, totalItems);
    }
}
