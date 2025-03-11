using FluentValidation;
using Hubtel.Insurance.API.DTOs;

namespace Hubtel.Insurance.API.Validators;

public class RequestQuoteValidator : AbstractValidator<RequestQuoteDTO>
{
    public RequestQuoteValidator()
    {
        RuleFor(x => x.PolicyId)
            .NotEmpty().WithMessage("Policy Id is required or cannot be zero")
            .GreaterThan(0).WithMessage("Policy Id must be greater than zero");

        RuleFor(x => x.MarketValue)
            .NotEmpty().WithMessage("Market Value is required or cannot be zero")
            .GreaterThan(0).WithMessage("Market Value must be greater than zero");
    }
}

