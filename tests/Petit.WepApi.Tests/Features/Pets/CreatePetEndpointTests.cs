using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Petit.WebApi.Data;
using Petit.WebApi.RequestModels;
using Petit.WepApi.Tests.Infrastructure;
using System.Net;
using System.Net.Http.Json;

namespace Petit.WepApi.Tests.Features.Pets;

[Collection("Pets")]
public class CreatePetEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public CreatePetEndpointTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _factory = factory;
    }

    [Fact]
    public async Task Post_Pet_Returns201_AndPersistsPet()
    {
        var request = new CreatePetRequest(
            "Buddy",
            "Dog",
            "Male",
            "Labrador",
            new DateOnly(2020, 5, 15).ToDateTime(TimeOnly.MinValue),
            30.5m,
            true,
            "A friendly dog");

        var response = await _client.PostAsJsonAsync("/api/pets", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PetDbContext>();
        var persisted = await context.Pets.Where(p => p.Name == "Buddy").FirstOrDefaultAsync();

        persisted.Should().NotBeNull();
        persisted!.Name.Should().Be("Buddy");
        persisted.Type.Should().Be("Dog");
        persisted.Gender.Should().Be("Male");
        persisted.Breed.Should().Be("Labrador");
        persisted.Birthday.Should().Be(new DateOnly(2020, 5, 15).ToDateTime(TimeOnly.MinValue));
        persisted.Weight.Should().Be(30.5m);
        persisted.Neutered.Should().BeTrue();
        persisted.Description.Should().Be("A friendly dog");
    }

    [Fact]
    public async Task Post_Pet_WithMissingName_Returns400()
    {
        var request = new CreatePetRequest(
            "",
            "Dog",
            "Male",
            null,
            null,
            null,
            null,
            null);

        var response = await _client.PostAsJsonAsync("/api/pets", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_Pet_WithMissingType_Returns400()
    {
        var request = new CreatePetRequest(
            "Buddy",
            "",
            "Male",
            null,
            null,
            null,
            null,
            null);

        var response = await _client.PostAsJsonAsync("/api/pets", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_Pet_WithMissingGender_Returns400()
    {
        var request = new CreatePetRequest(
            "Buddy",
            "Dog",
            "",
            null,
            null,
            null,
            null,
            null);

        var response = await _client.PostAsJsonAsync("/api/pets", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
