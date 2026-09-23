using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Petit.WebApi.Data;
using Petit.WebApi.Features.Pets;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var isTesting = builder.Environment.IsEnvironment("Testing");

if (!isTesting)
{
    builder.Services.AddDbContext<PetDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("PetDbConnection")));
}
else
{
    // For testing: use InMemory database (requires Microsoft.EntityFrameworkCore.InMemory package in test project)
    // Note: The TestWebApplicationFactory overrides this with a shared singleton context.
    // We keep this registration here only to satisfy the builder, but it will be removed by the factory.
    builder.Services.AddDbContext<PetDbContext>(options =>
        options.UseInMemoryDatabase("__PetDb_Test__"));
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Keycloak:Authority"] ?? throw new InvalidOperationException("Keycloak:Authority is not configured.");
        options.Audience = builder.Configuration["Keycloak:Audience"] ?? throw new InvalidOperationException("Keycloak:Audience is not configured.");
    });

builder.Services.AddAuthorization();

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthentication();
app.UseAuthorization();

app.MapCreatePet();
app.MapListPets();
app.MapGetPet();
app.MapUpdatePet();
app.MapDeletePet();

app.Run();
