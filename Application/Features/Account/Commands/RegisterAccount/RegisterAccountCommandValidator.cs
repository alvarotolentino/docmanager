using FluentValidation;
using FluentValidation.Validators;

namespace Application.Features.Account.Commands.RegisterAccount
{
    public class RegisterAccountCommandValidator : AbstractValidator<RegisterAccountCommand>
    {
        public RegisterAccountCommandValidator()
        {
            RuleFor(p => p.FirstName)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .NotNull();
            RuleFor(p => p.LastName)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .NotNull();
            RuleFor(p => p.Email)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .NotNull()
            .EmailAddress(EmailValidationMode.Net4xRegex)
            .WithMessage("A valid email is required");
            RuleFor(p => p.UserName)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .NotNull()
            .MinimumLength(6);
            RuleFor(p => p.Password)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .NotNull()
            .MinimumLength(6);
            RuleFor(p => p.ConfirmPassword)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .NotNull()
            .Equal(p => p.Password)
            .WithMessage("Passwords must match");

        }
    }
}