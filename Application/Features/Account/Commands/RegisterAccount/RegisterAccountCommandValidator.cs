using FluentValidation;
using FluentValidation.Validators;

namespace Application.Features.Account.Commands.RegisterAccount
{
    public class RegisterAccountCommandValidator : AbstractValidator<RegisterAccountCommand>
    {
        public RegisterAccountCommandValidator()
        {
            RuleFor(p => p.firstname)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .NotNull();
            RuleFor(p => p.lastname)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .NotNull();
            RuleFor(p => p.email)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .NotNull()
            .EmailAddress(EmailValidationMode.Net4xRegex)
            .WithMessage("A valid email is required");
            RuleFor(p => p.username)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .NotNull()
            .MinimumLength(6);
            RuleFor(p => p.password)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .NotNull()
            .MinimumLength(6);
            RuleFor(p => p.confirmpassword)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .NotNull()
            .Equal(p => p.password)
            .WithMessage("Passwords must match");

        }
    }
}