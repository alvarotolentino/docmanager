using FluentValidation;

namespace Application.Features.Documents.Commands.DeleteDocument
{
    public class DeleteDocumentByIdValidator : AbstractValidator<DeleteDocumentById>
    {
        public DeleteDocumentByIdValidator()
        {
            var validation = Resources.Reader.GetMessages()["Validation"];

            RuleFor(p => p.Id)
            .NotNull()
            .GreaterThan(0)
            .WithMessage((string)validation["DocumentIdMsg"]);
        }

    }
}