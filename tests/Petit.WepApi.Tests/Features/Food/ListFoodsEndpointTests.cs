using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Petit.WebApi.Data;
using Petit.WepApi.Tests.Infrastructure;
using System.Net;
using System.Net.Http.Json;

namespace Petit.WepApi.Tests.Features.Food;

[Collection("Food")]
public class ListFoodsEndpointTests : IClassFixture<TestWebApplicationFactory>, IDisposable
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public ListFoodsEndpointTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _factory = factory;
        ClearDatabase();
    }

    public void Dispose()
    {
        ClearDatabase();
    }

    private void ClearDatabase()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PetitDbContext>();
        context.Foods.RemoveRange(context.Foods);
        context.SaveChanges();
    }

    [Fact]
    public async Task Get_Foods_Returns200_WithPaginatedData()
    {
        ClearDatabase();
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PetitDbContext>();

        await context.Foods.AddRangeAsync(
            new Petit.WebApi.Data.Food { Id = Guid.NewGuid(), Name = "Chicken", Producer = "Farm A", Type = "Raw", PetType = "Dog", CaloriesPer100g = 165, ServingSize = 100, OwnerId = "test-user", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Petit.WebApi.Data.Food { Id = Guid.NewGuid(), Name = "Kibble", Producer = "Farm B", Type = "Dry", PetType = "Cat", CaloriesPer100g = 350, ServingSize = 50, OwnerId = "test-user", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Petit.WebApi.Data.Food { Id = Guid.NewGuid(), Name = "Salmon", Producer = "Farm C", Type = "Fish", PetType = "Dog", CaloriesPer100g = 208, ServingSize = 80, OwnerId = "test-user", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var response = await _client.GetAsync("/api/foods");

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new Exception($"Expected 200 but got {response.StatusCode}. Body: {body}");
        }

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json);
        result.Should().NotBeNull();
        ((System.Text.Json.JsonElement)result["totalItems"]).GetInt32().Should().Be(3);
    }

    [Fact]
    public async Task Get_Foods_WithTypeFilter_ReturnsFilteredResults()
    {
        ClearDatabase();
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PetitDbContext>();

        await context.Foods.AddRangeAsync(
            new Petit.WebApi.Data.Food { Id = Guid.NewGuid(), Name = "Chicken", Producer = "Farm A", Type = "Raw", PetType = "Dog", CaloriesPer100g = 165, ServingSize = 100, OwnerId = "test-user", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Petit.WebApi.Data.Food { Id = Guid.NewGuid(), Name = "Kibble", Producer = "Farm B", Type = "Dry", PetType = "Cat", CaloriesPer100g = 350, ServingSize = 50, OwnerId = "test-user", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var response = await _client.GetAsync("/api/foods?type=Dry");

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new Exception($"Expected 200 but got {response.StatusCode}. Body: {body}");
        }

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json);
        result.Should().NotBeNull();
        ((System.Text.Json.JsonElement)result["totalItems"]).GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task Get_Foods_WithPagination_ReturnsCorrectPageSize()
    {
        ClearDatabase();
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PetitDbContext>();

        for (var i = 0; i < 5; i++)
        {
            await context.Foods.AddAsync(new Petit.WebApi.Data.Food
            {
                Id = Guid.NewGuid(),
                Name = $"Food{i}",
                Producer = "Farm A",
                Type = "Raw",
                PetType = "Dog",
                CaloriesPer100g = 165,
                ServingSize = 100,
                OwnerId = "test-user",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
        await context.SaveChangesAsync();

        var response = await _client.GetAsync("/api/foods?page=0&pageSize=2");

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new Exception($"Expected 200 but got {response.StatusCode}. Body: {body}");
        }

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json);
        result.Should().NotBeNull();
        ((System.Text.Json.JsonElement)result["totalItems"]).GetInt32().Should().Be(5);
    }
}
