using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Petit.WebApi.Data;
using Petit.WepApi.Tests.Infrastructure;
using System.Net;
using System.Net.Http.Json;

namespace Petit.WepApi.Tests.Features.Food;

[Collection("Food")]
public class GetFoodEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public GetFoodEndpointTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _factory = factory;
    }

    [Fact]
    public async Task Get_Food_WithExistingId_Returns200()
    {
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

        var context = _factory.Services.GetRequiredService<PetitDbContext>();
        context.Foods.Add(food);
        await context.SaveChangesAsync();

        var response = await _client.GetAsync($"/api/foods/{food.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var result = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(json);
        result.Should().NotBeNull();
        result!.GetProperty("name").GetString().Should().Be("Chicken Breast");
        result.GetProperty("producer").GetString().Should().Be("Farm Fresh");
        result.GetProperty("type").GetString().Should().Be("Raw");
        result.GetProperty("petType").GetString().Should().Be("Dog");
        result.GetProperty("caloriesPer100g").GetInt32().Should().Be(165);
    }

    [Fact]
    public async Task Get_Food_WithNonExistingId_Returns404()
    {
        var response = await _client.GetAsync($"/api/foods/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
