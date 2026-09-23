using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Petit.WebApi.Data;
using Petit.WebApi.Features.Pets.Handlers;
using Petit.WebApi.Features.Pets.Validators;
using Petit.WebApi.RequestModels;
using System.Security.Claims;

namespace Petit.WebApi.Features.Pets;

public static class CreatePetEndpoint
{
    public static IEndpointRouteBuilder MapCreatePet(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/pets", async (
                [FromBody] CreatePetRequest request,
                [FromServices] IValidator<CreatePetRequest> validator,
                [FromServices] PetDbContext context,
                HttpContext httpContext) =>
            {
                var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

                var validationResult = await validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return Results.BadRequest(new
                    {
                        errors = validationResult.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage })
                    });
                }

                var (statusCode, data) = await CreatePetHandler.CreateAsync(context, request, userId);

                if (statusCode == 201)
                {
                    return Results.Created($"/api/pets/{data.Id}", data);
                }

                return Results.StatusCode(statusCode);
            })
            .RequireAuthorization();

        return app;
    }
}
