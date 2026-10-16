using FluentValidation;
using MeOne.Application.Communication.WorkInstructions.Requests;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.WorkInstructions;

public sealed class SaveWorkInstructionRequestValidator : AbstractValidator<SaveWorkInstructionRequest>
{
    public SaveWorkInstructionRequestValidator()
    {
        RuleFor(x => x.Code).Required(WorkInstructionErrors.CodeRequired, "O código", WorkInstructionErrors.MaxCodeLength);
        RuleFor(x => x.Title).Required(WorkInstructionErrors.TitleRequired, "O título", WorkInstructionErrors.MaxTitleLength);
        RuleFor(x => x.DocumentNumber)
            .Required(WorkInstructionErrors.DocumentNumberRequired, "O número do documento", WorkInstructionErrors.MaxDocumentNumberLength);
        RuleFor(x => x.Product).Required(WorkInstructionErrors.ProductRequired, "O produto", WorkInstructionErrors.MaxProductLength);
        RuleFor(x => x.PreparedBy)
            .Required(WorkInstructionErrors.PreparedByRequired, "O campo elaborado por", WorkInstructionErrors.MaxPersonLength);
        RuleFor(x => x.ApprovedBy)
            .Required(WorkInstructionErrors.ApprovedByRequired, "O campo aprovado por", WorkInstructionErrors.MaxPersonLength);
        RuleFor(x => x.RequiredPpe).Required(WorkInstructionErrors.PpeRequired, "Os EPIs", WorkInstructionErrors.MaxPpeLength);
        RuleFor(x => x.ProcessSpecifications)
            .Required(WorkInstructionErrors.SpecificationsRequired, "As especificações", WorkInstructionErrors.MaxSpecificationsLength);

        RuleFor(x => x.Notes)
            .Must(notes => notes!.Trim().Length <= WorkInstructionErrors.MaxNotesLength)
            .When(x => x.Notes is not null)
            .WithMessage(WorkInstructionErrors.FieldTooLong("As observações", WorkInstructionErrors.MaxNotesLength));

        RuleFor(x => x.Steps)
            .Must(steps => steps!.Length <= WorkInstructionErrors.MaxSteps)
            .When(x => x.Steps is not null)
            .WithMessage(WorkInstructionErrors.TooManySteps);

        RuleForEach(x => x.Steps).ChildRules(step =>
        {
            step.RuleFor(item => item.Text)
                .NotEmpty().WithMessage(WorkInstructionErrors.StepTextRequired)
                .Must(text => text.Trim().Length <= WorkInstructionErrors.MaxStepTextLength)
                .WithMessage(WorkInstructionErrors.FieldTooLong("O texto do passo", WorkInstructionErrors.MaxStepTextLength));
        });
    }
}
