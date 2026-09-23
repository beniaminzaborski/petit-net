using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Petit.WebApi.Data;
using Petit.WepApi.Tests.Infrastructure;
using System.Net;
using System.Net.Http.Json;

namespace Petit.WepApi.Tests.Features.Pets;

[Collection("Pets")]
public class GetPetEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public GetPetEndpointTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _factory = factory;
    }

    [Fact]
    public async Task Get_Pet_WithExistingId_Returns200()
    {
        var pet = new Pet
        {
            Id = Guid.NewGuid(),
            Name = "Buddy",
            Type = "Dog",
            Gender = "Male",
            Breed = "Labrador",
            OwnerId = "test-user",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var context = _factory.Services.GetRequiredService<PetDbContext>();
        Console.WriteLine($"Test context: {context.GetHashCode()}");
        context.Pets.Add(pet);
        await context.SaveChangesAsync();
        Console.WriteLine($"Saved pet: {pet.Id}, Count: {context.Pets.Count()}");

        var response = await _client.GetAsync($"/api/pets/{pet.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var result = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(json);
        result.Should().NotBeNull();
        result.GetProperty("name").GetString().Should().Be("Buddy");
        result.GetProperty("type").GetString().Should().Be("Dog");
    }

    [Fact]
    public async Task Get_Pet_WithNonExistingId_Returns404()
    {
        var response = await _client.GetAsync($"/api/pets/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
