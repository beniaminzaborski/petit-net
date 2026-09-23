using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Petit.WebApi.Data;
using Petit.WebApi.Features.Pets.Handlers;
using Petit.WebApi.Features.Pets.Validators;
using Petit.WebApi.RequestModels;
using System.Security.Claims;

namespace Petit.WebApi.Features.Pets;

public static class UpdatePetEndpoint
{
    public static IEndpointRouteBuilder MapUpdatePet(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/pets/{id:guid}", async (
                Guid id,
                [FromBody] UpdatePetRequest request,
                [FromServices] IValidator<UpdatePetRequest> validator,
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

                var (statusCode, data) = await UpdatePetHandler.UpdateAsync(context, id, request, userId);

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
