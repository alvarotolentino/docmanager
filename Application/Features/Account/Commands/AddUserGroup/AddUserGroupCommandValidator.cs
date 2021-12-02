using FluentValidation;

namespace Application.Features.Account.Commands.AddUserGroup
{
    public class AddUserGroupValidator : AbstractValidator<AddUserGroup>
    {
        public AddUserGroupValidator()
        {
            RuleFor(p => p.UserId)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .NotNull()
            .GreaterThan(0);

            RuleFor(p => p.GroupId)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .NotNull()
            .GreaterThan(0);
        }
    }
}