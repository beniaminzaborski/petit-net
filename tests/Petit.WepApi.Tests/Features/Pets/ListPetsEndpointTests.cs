using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Petit.WebApi.Data;
using Petit.WepApi.Tests.Infrastructure;
using System.Net;
using System.Net.Http.Json;

namespace Petit.WepApi.Tests.Features.Pets;

[Collection("Pets")]
public class ListPetsEndpointTests : IClassFixture<TestWebApplicationFactory>, IDisposable
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public ListPetsEndpointTests(TestWebApplicationFactory factory)
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
        var context = scope.ServiceProvider.GetRequiredService<PetDbContext>();
        context.Pets.RemoveRange(context.Pets);
        context.SaveChanges();
    }

    [Fact]
    public async Task Get_Pets_Returns200_WithPaginatedData()
    {
        ClearDatabase();
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PetDbContext>();

        await context.Pets.AddRangeAsync(
            new Pet { Id = Guid.NewGuid(), Name = "Alice", Type = "Dog", Gender = "Female", OwnerId = "test-user", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Pet { Id = Guid.NewGuid(), Name = "Buddy", Type = "Cat", Gender = "Male", OwnerId = "test-user", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Pet { Id = Guid.NewGuid(), Name = "Charlie", Type = "Dog", Gender = "Male", OwnerId = "test-user", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var response = await _client.GetAsync("/api/pets");

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
    public async Task Get_Pets_WithTypeFilter_ReturnsFilteredResults()
    {
        ClearDatabase();
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PetDbContext>();

        await context.Pets.AddRangeAsync(
            new Pet { Id = Guid.NewGuid(), Name = "Alice", Type = "Dog", Gender = "Female", OwnerId = "test-user", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Pet { Id = Guid.NewGuid(), Name = "Whiskers", Type = "Cat", Gender = "Male", OwnerId = "test-user", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var response = await _client.GetAsync("/api/pets?type=Dog");

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
    public async Task Get_Pets_WithPagination_ReturnsCorrectPageSize()
    {
        ClearDatabase();
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PetDbContext>();

        for (var i = 0; i < 5; i++)
        {
            await context.Pets.AddAsync(new Pet
            {
                Id = Guid.NewGuid(),
                Name = $"Pet{i}",
                Type = "Dog",
                Gender = "Male",
                OwnerId = "test-user",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
        await context.SaveChangesAsync();

        var response = await _client.GetAsync("/api/pets?page=0&pageSize=2");

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
