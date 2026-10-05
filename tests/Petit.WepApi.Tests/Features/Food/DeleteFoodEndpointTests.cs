using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Petit.WebApi.Data;
using Petit.WepApi.Tests.Infrastructure;
using System.Net;

namespace Petit.WepApi.Tests.Features.Food;

[Collection("Food")]
public class DeleteFoodEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public DeleteFoodEndpointTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _factory = factory;
    }

    [Fact]
    public async Task Delete_Food_WithExistingId_Returns204()
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

        var response = await _client.DeleteAsync($"/api/foods/{food.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var newScope = _factory.Services.CreateScope();
        var newContext = newScope.ServiceProvider.GetRequiredService<PetitDbContext>();
        var deleted = await newContext.Foods.FindAsync(food.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task Delete_Food_WithNonExistingId_Returns404()
    {
        var response = await _client.DeleteAsync($"/api/foods/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
