namespace Petit.WebApi.Data;

public class Food
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Producer { get; set; } = "";
    public string Type { get; set; } = "";
    public string? PetType { get; set; }
    public int CaloriesPer100g { get; set; }
    public int ServingSize { get; set; }
    public string? SubCategory { get; set; }
    public string? Description { get; set; }
    public string OwnerId { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
