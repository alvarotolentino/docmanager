using FluentValidation;

namespace Application.Features.Groups.Commands.UpdateGroup
{
    public class UpdateGroupValidator : AbstractValidator<UpdateGroup>
    {
        public UpdateGroupValidator()
        {
            RuleFor(p => p.id)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .NotNull()
            .GreaterThan(0);

            RuleFor(p => p.name)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .NotNull();
        }
    }
}