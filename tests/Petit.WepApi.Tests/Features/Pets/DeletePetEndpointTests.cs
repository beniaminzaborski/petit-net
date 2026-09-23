using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Petit.WebApi.Data;
using Petit.WepApi.Tests.Infrastructure;
using System.Net;

namespace Petit.WepApi.Tests.Features.Pets;

[Collection("Pets")]
public class DeletePetEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public DeletePetEndpointTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _factory = factory;
    }

    [Fact]
    public async Task Delete_Pet_WithExistingId_Returns204()
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

        var response = await _client.DeleteAsync($"/api/pets/{pet.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var newScope = _factory.Services.CreateScope();
        var newContext = newScope.ServiceProvider.GetRequiredService<PetDbContext>();
        var deleted = await newContext.Pets.FindAsync(pet.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task Delete_Pet_WithNonExistingId_Returns404()
    {
        var response = await _client.DeleteAsync($"/api/pets/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
