using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Petit.WebApi.Data;
using Petit.WebApi.Features.Food.Handlers;
using System.Security.Claims;

namespace Petit.WebApi.Features.Food;

public static class GetFoodEndpoint
{
    public static IEndpointRouteBuilder MapGetFood(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/foods/{id:guid}", async (
                Guid id,
                [FromServices] PetitDbContext context,
                HttpContext httpContext) =>
            {
                var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

                var (statusCode, data) = await GetFoodHandler.GetByIdAsync(context, id, userId);

                if (statusCode == 404)
                {
                    return Results.NotFound(new
                    {
                        error = "Food not found or does not belong to the user."
                    });
                }

                return Results.Ok(data);
            })
            .RequireAuthorization();

        return app;
    }
}
