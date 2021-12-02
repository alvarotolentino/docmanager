using FluentValidation;

namespace Application.Features.Groups.Commands.RegisterGroup
{
    public class RegisterGroupValidator : AbstractValidator<RegisterGroup>
    {
        public RegisterGroupValidator()
        {
            RuleFor(p => p.name)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .NotNull();
        }
    }
}