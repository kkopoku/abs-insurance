using FluentValidation;
using Hubtel.Insurance.API.DTOs;

namespace Hubtel.Insurance.API.Validators
{
    public class CreatePolicyValidator : AbstractValidator<CreatePolicyDTO>
    {
        public CreatePolicyValidator()
        {
            RuleFor(x => x.PolicyId)
                .GreaterThan(0).WithMessage("Policy Id is required and must be greater than zero");

            RuleFor(x => x.Policy)
                .NotEmpty().WithMessage("Policy name is required");

            RuleFor(x => x.Components)
                .NotNull().WithMessage("Exactly 4 components are required")
                .Must(c => c.Count == 4).WithMessage("Exactly 4 components are required");
        }
    }
}