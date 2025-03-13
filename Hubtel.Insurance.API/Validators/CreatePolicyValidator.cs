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

            RuleForEach(x => x.Components).ChildRules(components =>
            {
                components.RuleFor(c => c.PercentageValue)
                    .GreaterThan(0)
                    .When(c => c.PercentageValue.HasValue)
                    .WithMessage("Percentage value must be between 0 and 100");

                components.RuleFor(c => c.FlatValue)
                    .GreaterThan(0)
                    .When(c => c.FlatValue.HasValue)
                    .WithMessage("Flat value must be greater than 0");

                components.RuleFor(c => c)
                    .Must(c => (c.PercentageValue ?? 0) == 0 || (c.FlatValue ?? 0) == 0)
                    .WithMessage("A component cannot have both percentage and flat value set");
            });
        }
    }
}