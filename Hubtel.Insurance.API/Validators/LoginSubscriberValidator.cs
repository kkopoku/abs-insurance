namespace Hubtel.Insurance.API.Validators;


using Hubtel.Insurance.API.DTOs;
using FluentValidation;


public class LoginSubscriberValidator : AbstractValidator<SubscriberDTO>{

    public LoginSubscriberValidator(
        ILogger<LoginSubscriberValidator> logger

    ){
        
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long");

    }

}