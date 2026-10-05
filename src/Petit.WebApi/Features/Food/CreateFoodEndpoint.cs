using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Petit.WebApi.Data;
using Petit.WebApi.Features.Food.Handlers;
using Petit.WebApi.Features.Food.Validators;
using Petit.WebApi.RequestModels;
using System.Security.Claims;

namespace Petit.WebApi.Features.Food;

public static class CreateFoodEndpoint
{
    public static IEndpointRouteBuilder MapCreateFood(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/foods", async (
                [FromBody] CreateFoodRequest request,
                [FromServices] IValidator<CreateFoodRequest> validator,
                [FromServices] PetitDbContext context,
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

                var (statusCode, data) = await CreateFoodHandler.CreateAsync(context, request, userId);

                if (statusCode == 201)
                {
                    return Results.Created($"/api/foods/{data.Id}", data);
                }

                return Results.StatusCode(statusCode);
            })
            .RequireAuthorization();

        return app;
    }
}
