namespace Petit.WebApi.Features.Pets;

public sealed record PetResponse(
    Guid Id,
    string Name,
    string Type,
    string Gender,
    string? Breed,
    DateTime? Birthday,
    decimal? Weight,
    bool? Neutered,
    string? Description,
    string OwnerId,
    DateTime CreatedAt,
    DateTime UpdatedAt);
