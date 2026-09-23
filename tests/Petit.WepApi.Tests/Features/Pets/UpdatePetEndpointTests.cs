using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Petit.WebApi.Data;
using Petit.WebApi.RequestModels;
using Petit.WepApi.Tests.Infrastructure;
using System.Net;
using System.Net.Http.Json;

namespace Petit.WepApi.Tests.Features.Pets;

[Collection("Pets")]
public class UpdatePetEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public UpdatePetEndpointTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _factory = factory;
    }

    [Fact]
    public async Task Put_Pet_WithExistingId_Returns200_AndUpdatesData()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PetDbContext>();

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
        context.Pets.Add(pet);
        await context.SaveChangesAsync();

        var request = new UpdatePetRequest(
            "Max",
            "Dog",
            "Male",
            "Golden Retriever",
            null,
            null,
            null,
            "Updated description");

        var response = await _client.PutAsJsonAsync($"/api/pets/{pet.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var jsonElement = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        jsonElement.Should().NotBeNull();
        var nameProp = jsonElement!.GetProperty("name");
        var breedProp = jsonElement.GetProperty("breed");
        nameProp.GetString().Should().Be("Max");
        breedProp.GetString().Should().Be("Golden Retriever");
    }

    [Fact]
    public async Task Put_Pet_WithNonExistingId_Returns404()
    {
        var request = new UpdatePetRequest(
            "Max",
            "Dog",
            "Male",
            null,
            null,
            null,
            null,
            null);

        var response = await _client.PutAsJsonAsync($"/api/pets/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Put_Pet_WithMissingName_Returns400()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PetDbContext>();

        var pet = new Pet
        {
            Id = Guid.NewGuid(),
            Name = "Buddy",
            Type = "Dog",
            Gender = "Male",
            OwnerId = "test-user",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Pets.Add(pet);
        await context.SaveChangesAsync();

        var request = new UpdatePetRequest(
            "",
            "Dog",
            "Male",
            null,
            null,
            null,
            null,
            null);

        var response = await _client.PutAsJsonAsync($"/api/pets/{pet.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
