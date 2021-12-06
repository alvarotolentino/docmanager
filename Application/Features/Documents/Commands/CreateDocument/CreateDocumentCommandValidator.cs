using FluentValidation;

namespace Application.Features.Documents.Commands.CreateDocument
{
    public class CreateDocumentValidator : AbstractValidator<CreateDocument>
    {
        public CreateDocumentValidator()
        {

            var validation = Resources.Reader.GetMessages()["Validation"];

            RuleFor(p => p.File)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .NotNull()
            .Must(f => f.Length < (long)validation["MaxFileSize"])
            .WithMessage((string)validation["MaxFileSizeMsg"]);

            RuleFor(p => p.Description)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .NotNull()
            .MaximumLength((int)validation["DescriptionLength"]).WithMessage((string)validation["DescriptionMsg"]);

            RuleFor(p => p.Category)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .NotNull()
            .MaximumLength((int)validation["CategoryLength"]).WithMessage((string)validation["CategoryMsg"]);
        }
    }
}