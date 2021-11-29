using FluentValidation;

namespace Application.Features.Documents.Commands.AssignUserPermission
{
    public class AssignUserPermissionCommandValidator: AbstractValidator<AssignUserPermissionCommand>
    {
        public AssignUserPermissionCommandValidator()
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