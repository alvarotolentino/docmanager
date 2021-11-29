using FluentValidation;

namespace Application.Features.Documents.Commands.AssignGroupPermission
{
    public class AssignGroupPermissionCommandValidator : AbstractValidator<AssignGroupPermissionCommand>
    {
        public AssignGroupPermissionCommandValidator()
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