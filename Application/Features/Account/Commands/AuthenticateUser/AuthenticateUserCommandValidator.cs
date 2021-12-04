using FluentValidation;
using FluentValidation.Validators;

namespace Application.Features.Account.Commands.AuthenticateUser
{
    public class AuthenticateUserValidator : AbstractValidator<AuthenticateUser>
    {
        public AuthenticateUserValidator()
        {
            RuleFor(p => p.Email)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .NotNull()
            .EmailAddress(EmailValidationMode.Net4xRegex)
            .WithMessage("A valid email is required");

            RuleFor(p => p.Password)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .NotNull();
        }
    }
}