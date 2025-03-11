namespace Hubtel.Insurance.API.Validators;

using FluentValidation;
using Hubtel.Insurance.API.DTOs;

public class UpdatePolicyValidator : AbstractValidator<UpdatePolicyDTO>
{
    public UpdatePolicyValidator()
    {
        RuleFor(x => x.PolicyId)
            .NotEmpty().WithMessage("Policy ID is required.");

        RuleForEach(x => x.Components).SetValidator(new UpdatePolicyComponentValidator());
    }
}

public class UpdatePolicyComponentValidator : AbstractValidator<UpdatePolicyComponentDTO>
{
    public UpdatePolicyComponentValidator()
    {
        RuleFor(x => x.Sequence)
            .InclusiveBetween(1, 4).WithMessage("Sequence must be between 1 and 4");

        RuleFor(x => x)
            .Must(x => (x.FlatValue ?? 0) == 0 || (x.PercentageValue ?? 0) == 0)
            .WithMessage("Cannot have both FlatValue and PercentageValue in a component.");
    }
}
