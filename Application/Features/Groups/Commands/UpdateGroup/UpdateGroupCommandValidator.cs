using FluentValidation;

namespace Application.Features.Groups.Commands.UpdateGroup
{
    public class UpdateGroupValidator : AbstractValidator<UpdateGroup>
    {
        public UpdateGroupValidator()
        {
            RuleFor(p => p.Id)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .NotNull()
            .GreaterThan(0);

            RuleFor(p => p.Name)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .NotNull();
        }
    }
}