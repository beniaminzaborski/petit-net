namespace Petit.WebApi.RequestModels;

public sealed record CreatePetRequest(
    string Name,
    string Type,
    string Gender,
    string? Breed,
    DateTime? Birthday,
    decimal? Weight,
    bool? Neutered,
    string? Description);
