using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Petit.WebApi.Data;
using Petit.WebApi.Features.Food.Handlers;
using Petit.WebApi.Features.Food.Validators;
using Petit.WebApi.RequestModels;
using System.Security.Claims;

namespace Petit.WebApi.Features.Food;

public static class UpdateFoodEndpoint
{
    public static IEndpointRouteBuilder MapUpdateFood(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/foods/{id:guid}", async (
                Guid id,
                [FromBody] UpdateFoodRequest request,
                [FromServices] IValidator<UpdateFoodRequest> validator,
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

                var (statusCode, data) = await UpdateFoodHandler.UpdateAsync(context, id, request, userId);

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
