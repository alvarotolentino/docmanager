using FluentValidation;

namespace Application.Features.Groups.Commands.DeleteGroup
{
    public class DeleteGroupValidator : AbstractValidator<DeleteGroup>
    {
        public DeleteGroupValidator()
        {
            RuleFor(p => p.Id)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .NotNull()
            .GreaterThan(0);
        }

    }
}