using FluentValidation;
using Petit.WebApi.RequestModels;

namespace Petit.WebApi.Features.Pets.Validators;

public sealed class UpdatePetValidator : AbstractValidator<UpdatePetRequest>
{
    public UpdatePetValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.Type).NotEmpty().WithMessage("Type is required.");
        RuleFor(x => x.Gender).NotEmpty().WithMessage("Gender is required.");
    }
}
