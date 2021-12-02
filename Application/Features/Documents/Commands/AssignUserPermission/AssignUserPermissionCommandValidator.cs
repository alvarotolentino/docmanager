using FluentValidation;

namespace Application.Features.Documents.Commands.AssignUserPermission
{
    public class AssignUserPermissionValidator: AbstractValidator<AssignUserPermission>
    {
        public AssignUserPermissionValidator()
        {
            RuleFor(p => p.UserId)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .NotNull()
            .GreaterThan(0);

            RuleFor(p => p.DocumentId)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .NotNull()
            .GreaterThan(0);
        }
    }
}