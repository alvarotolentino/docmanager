using FluentValidation;

namespace Application.Features.Documents.Commands.AssignGroupPermission
{
    public class AssignGroupPermissionValidator : AbstractValidator<AssignGroupPermission>
    {
        public AssignGroupPermissionValidator()
        {
            RuleFor(p => p.GroupId)
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