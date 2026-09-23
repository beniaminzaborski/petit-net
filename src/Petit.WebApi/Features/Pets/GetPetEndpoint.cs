using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Petit.WebApi.Data;
using Petit.WebApi.Features.Pets.Handlers;
using System.Security.Claims;

namespace Petit.WebApi.Features.Pets;

public static class GetPetEndpoint
{
    public static IEndpointRouteBuilder MapGetPet(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/pets/{id:guid}", async (
                Guid id,
                [FromServices] PetDbContext context,
                HttpContext httpContext) =>
            {
                var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

                var (statusCode, data) = await GetPetHandler.GetByIdAsync(context, id, userId);

                if (statusCode == 404)
                {
                    return Results.NotFound(new
                    {
                        error = "Pet not found or does not belong to the user."
                    });
                }

                return Results.Ok(data);
            })
            .RequireAuthorization();

        return app;
    }
}
