using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Petit.WebApi.Data;
using Petit.WebApi.Features.Food.Handlers;
using System.Security.Claims;

namespace Petit.WebApi.Features.Food;

public static class ListFoodsEndpoint
{
    public static IEndpointRouteBuilder MapListFoods(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/foods", async (
                HttpContext httpContext,
                [FromQuery] int page = 0,
                [FromQuery] int pageSize = 10,
                [FromQuery] string type = default!,
                [FromServices] PetitDbContext context = default!) =>
            {
                var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

                var (statusCode, data, totalItems) = await ListFoodsHandler.ListAsync(context, userId, page, pageSize, type);

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
