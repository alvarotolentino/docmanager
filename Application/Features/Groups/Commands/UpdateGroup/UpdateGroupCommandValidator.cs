using FluentValidation;

namespace Application.Features.Groups.Commands.UpdateGroup
{
    public class UpdateGroupCommandValidator : AbstractValidator<UpdateGroupCommand>
    {
        public UpdateGroupCommandValidator()
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