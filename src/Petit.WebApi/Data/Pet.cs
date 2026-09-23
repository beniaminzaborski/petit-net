namespace Petit.WebApi.Data;

public class Pet
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string Gender { get; set; } = "";
    public string? Breed { get; set; }
    public DateTime? Birthday { get; set; }
    public decimal? Weight { get; set; }
    public bool? Neutered { get; set; }
    public string? Description { get; set; }
    public string OwnerId { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
