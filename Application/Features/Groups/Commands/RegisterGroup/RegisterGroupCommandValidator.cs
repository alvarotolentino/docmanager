using FluentValidation;

namespace Application.Features.Groups.Commands.RegisterGroup
{
    public class RegisterGroupCommandValidator : AbstractValidator<RegisterGroupCommand>
    {
        public RegisterGroupCommandValidator()
        {
            RuleFor(p => p.name)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .NotNull();
        }
    }
}