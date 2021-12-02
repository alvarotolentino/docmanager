using FluentValidation;

namespace Application.Features.Account.Commands.AddUserGroup
{
    public class AddUserGroupValidator : AbstractValidator<AddUserGroup>
    {
        public AddUserGroupValidator()
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