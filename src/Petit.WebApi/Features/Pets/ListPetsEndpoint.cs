using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Petit.WebApi.Data;
using Petit.WebApi.Features.Pets.Handlers;
using System.Security.Claims;

namespace Petit.WebApi.Features.Pets;

public static class ListPetsEndpoint
{
    public static IEndpointRouteBuilder MapListPets(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/pets", async (
                HttpContext httpContext,
                [FromQuery] int page = 0,
                [FromQuery] int pageSize = 10,
                [FromQuery] string type = default!,
                [FromServices] PetDbContext context = default!) =>
            {
                var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

                var (statusCode, data, totalItems) = await ListPetsHandler.ListAsync(context, userId, page, pageSize, type);

                return Results.Ok(new
                {
                    data,
                    page,
                    pageSize,
                    totalItems
                });
            })
            .RequireAuthorization();

        return app;
    }
}
