using FluentValidation;
using FluentValidation.Validators;

namespace Application.Features.Account.Commands.AuthenticateUser
{
    public class AuthenticateUserCommandValidator : AbstractValidator<AuthenticateUserCommand>
    {
        public AuthenticateUserCommandValidator()
        {
            RuleFor(p => p.email)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .NotNull()
            .EmailAddress(EmailValidationMode.Net4xRegex)
            .WithMessage("A valid email is required");

            RuleFor(p => p.password)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .NotNull();
        }
    }
}