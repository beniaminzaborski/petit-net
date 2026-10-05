using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Petit.WebApi.Data;
using Petit.WebApi.RequestModels;
using Petit.WepApi.Tests.Infrastructure;
using System.Net;
using System.Net.Http.Json;

namespace Petit.WepApi.Tests.Features.Food;

[Collection("Food")]
public class UpdateFoodEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public UpdateFoodEndpointTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _factory = factory;
    }

    [Fact]
    public async Task Put_Food_WithExistingId_Returns200_AndUpdatesData()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PetitDbContext>();

        var food = new Petit.WebApi.Data.Food
        {
            Id = Guid.NewGuid(),
            Name = "Chicken Breast",
            Producer = "Farm Fresh",
            Type = "Raw",
            PetType = "Dog",
            CaloriesPer100g = 165,
            ServingSize = 100,
            SubCategory = "Poultry",
            Description = "High protein meat",
            OwnerId = "test-user",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Foods.Add(food);
        await context.SaveChangesAsync();

        var request = new UpdateFoodRequest(
            "Turkey Breast",
            "Farm Fresh",
            "Raw",
            "Cat",
            135,
            120,
            "Poultry",
            "Lean poultry meat");

        var response = await _client.PutAsJsonAsync($"/api/foods/{food.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var jsonElement = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        jsonElement.Should().NotBeNull();
        var nameProp = jsonElement!.GetProperty("name");
        var petTypeProp = jsonElement.GetProperty("petType");
        nameProp.GetString().Should().Be("Turkey Breast");
        petTypeProp.GetString().Should().Be("Cat");
    }

    [Fact]
    public async Task Put_Food_WithNonExistingId_Returns404()
    {
        var request = new UpdateFoodRequest(
            "Turkey Breast",
            "Farm Fresh",
            "Raw",
            "Cat",
            135,
            120,
            "Poultry",
            "Lean poultry meat");

        var response = await _client.PutAsJsonAsync($"/api/foods/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Put_Food_WithMissingName_Returns400()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PetitDbContext>();

        var food = new Petit.WebApi.Data.Food
        {
            Id = Guid.NewGuid(),
            Name = "Chicken Breast",
            Producer = "Farm Fresh",
            Type = "Raw",
            PetType = "Dog",
            CaloriesPer100g = 165,
            ServingSize = 100,
            SubCategory = "Poultry",
            Description = "High protein meat",
            OwnerId = "test-user",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Foods.Add(food);
        await context.SaveChangesAsync();

        var request = new UpdateFoodRequest(
            "",
            "Farm Fresh",
            "Raw",
            "Cat",
            135,
            120,
            "Poultry",
            "Lean poultry meat");

        var response = await _client.PutAsJsonAsync($"/api/foods/{food.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Put_Food_WithMissingType_Returns400()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PetitDbContext>();

        var food = new Petit.WebApi.Data.Food
        {
            Id = Guid.NewGuid(),
            Name = "Chicken Breast",
            Producer = "Farm Fresh",
            Type = "Raw",
            PetType = "Dog",
            CaloriesPer100g = 165,
            ServingSize = 100,
            SubCategory = "Poultry",
            Description = "High protein meat",
            OwnerId = "test-user",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Foods.Add(food);
        await context.SaveChangesAsync();

        var request = new UpdateFoodRequest(
            "Turkey Breast",
            "Farm Fresh",
            "",
            "Cat",
            135,
            120,
            "Poultry",
            "Lean poultry meat");

        var response = await _client.PutAsJsonAsync($"/api/foods/{food.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
