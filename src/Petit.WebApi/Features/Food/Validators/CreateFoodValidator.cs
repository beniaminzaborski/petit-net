using FluentValidation;
using Petit.WebApi.RequestModels;

namespace Petit.WebApi.Features.Food.Validators;

public sealed class CreateFoodValidator : AbstractValidator<CreateFoodRequest>
{
    public CreateFoodValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.Producer).NotEmpty().WithMessage("Producer is required.");
        RuleFor(x => x.Type).NotEmpty().WithMessage("Type must be one of: Dry, Wet, Raw, Treats, Other.")
            .Must(t => t is "Dry" or "Wet" or "Raw" or "Treats" or "Other").WithMessage("Type must be one of: Dry, Wet, Raw, Treats, Other.");
        RuleFor(x => x.CaloriesPer100g).GreaterThan(0).LessThanOrEqualTo(10000).WithMessage("CaloriesPer100g must be between 1 and 10000.");
        RuleFor(x => x.ServingSize).GreaterThan(0).LessThanOrEqualTo(10000).WithMessage("ServingSize must be between 1 and 10000.");
        RuleFor(x => x.PetType)
            .Must(pt => pt is null or "Dog" or "Cat" or "Other").WithMessage("PetType must be one of: Dog, Cat, Other or null.");
    }
}
