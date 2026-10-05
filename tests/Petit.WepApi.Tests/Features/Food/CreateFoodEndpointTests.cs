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
public class CreateFoodEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public CreateFoodEndpointTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _factory = factory;
    }

    [Fact]
    public async Task Post_Food_Returns201_AndPersistsFood()
    {
        var request = new CreateFoodRequest(
            "Chicken Breast",
            "Farm Fresh",
            "Raw",
            "Dog",
            165,
            100,
            "Poultry",
            "High protein meat");

        var response = await _client.PostAsJsonAsync("/api/foods", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        Petit.WebApi.Data.Food? persisted = null;
        using (var scope = _factory.Services.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<PetitDbContext>();
            var foods = ctx.Set<Petit.WebApi.Data.Food>().AsQueryable().ToList();
            persisted = foods.FirstOrDefault(f => f.Name == "Chicken Breast");
        }

        persisted.Should().NotBeNull();
        persisted!.Name.Should().Be("Chicken Breast");
        persisted.Producer.Should().Be("Farm Fresh");
        persisted.Type.Should().Be("Raw");
        persisted.PetType.Should().Be("Dog");
        persisted.CaloriesPer100g.Should().Be(165);
        persisted.ServingSize.Should().Be(100);
        persisted.SubCategory.Should().Be("Poultry");
        persisted.Description.Should().Be("High protein meat");
    }

    [Fact]
    public async Task Post_Food_WithMissingName_Returns400()
    {
        var request = new CreateFoodRequest(
            "",
            "Farm Fresh",
            "Raw",
            "Dog",
            165,
            100,
            "Poultry",
            "High protein meat");

        var response = await _client.PostAsJsonAsync("/api/foods", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_Food_WithMissingType_Returns400()
    {
        var request = new CreateFoodRequest(
            "Chicken Breast",
            "Farm Fresh",
            "",
            "Dog",
            165,
            100,
            "Poultry",
            "High protein meat");

        var response = await _client.PostAsJsonAsync("/api/foods", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_Food_WithMissingProducer_Returns400()
    {
        var request = new CreateFoodRequest(
            "Chicken Breast",
            "",
            "Raw",
            "Dog",
            165,
            100,
            "Poultry",
            "High protein meat");

        var response = await _client.PostAsJsonAsync("/api/foods", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_Food_WithCaloriesOutOfRange_Returns400()
    {
        var request = new CreateFoodRequest(
            "Chicken Breast",
            "Farm Fresh",
            "Raw",
            "Dog",
            0,
            100,
            "Poultry",
            "High protein meat");

        var response = await _client.PostAsJsonAsync("/api/foods", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_Food_WithServingSizeOutOfRange_Returns400()
    {
        var request = new CreateFoodRequest(
            "Chicken Breast",
            "Farm Fresh",
            "Raw",
            "Dog",
            165,
            0,
            "Poultry",
            "High protein meat");

        var response = await _client.PostAsJsonAsync("/api/foods", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
