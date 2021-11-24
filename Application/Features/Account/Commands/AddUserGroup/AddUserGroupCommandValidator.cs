using FluentValidation;

namespace Application.Features.Account.Commands.AddUserGroup
{
    public class AddUserGroupCommandValidator : AbstractValidator<AddUserGroupCommand>
    {
        public AddUserGroupCommandValidator()
        {
            RuleFor(p => p.userid)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .NotNull()
            .GreaterThan(0);

            RuleFor(p => p.groupid)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .NotNull()
            .GreaterThan(0);
        }
    }
}