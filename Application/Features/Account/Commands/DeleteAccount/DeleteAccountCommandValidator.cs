using FluentValidation;

namespace Application.Features.Account.Commands.DeleteAccount
{
    public class DeleteAccountValidator : AbstractValidator<DeleteAccount>
    {
        public DeleteAccountValidator()
        {
            RuleFor(p => p.id)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .NotNull()
            .GreaterThan(0);
        }
    }
}