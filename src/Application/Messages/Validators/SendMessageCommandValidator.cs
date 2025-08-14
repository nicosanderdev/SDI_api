using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.Messages.Commands;

namespace SDI_Api.Application.Messages.Validators;

public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    private readonly IApplicationDbContext _context;

    public SendMessageCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(v => v.UserId)
            .NotEmpty().WithMessage("Sender user ID is required.");

        RuleFor(v => v.MessageData)
            .NotNull().WithMessage("Message data is required.");

        // The following rules are dependent on MessageData not being null.
        // The `When` clause prevents NullReferenceException if MessageData is null.
        When(v => true, () =>
        {
            RuleFor(v => v.MessageData.RecipientId)
                .NotEmpty().WithMessage("RecipientId is required.")
                .Must(BeAValidGuid).WithMessage("RecipientId must be a valid GUID.")
                .MustAsync(RecipientMustExist).WithMessage("The specified recipient does not exist.");

            RuleFor(v => v.MessageData.Body)
                .NotEmpty().WithMessage("Message body cannot be empty.");

            // A subject is required only when creating a new thread.
            RuleFor(v => v.MessageData.Subject)
                .NotEmpty().WithMessage("Subject is required for a new message thread.")
                .When(v => string.IsNullOrEmpty(v.MessageData.ThreadId) && string.IsNullOrEmpty(v.MessageData.InReplyToMessageId));

            // If a ThreadId is provided, it must be a valid GUID and exist.
            RuleFor(v => v.MessageData.ThreadId)
                .Must(BeAValidGuid).WithMessage("ThreadId must be a valid GUID.")
                .When(v => !string.IsNullOrEmpty(v.MessageData.ThreadId))
                .MustAsync(ThreadMustExist).WithMessage("The specified message thread does not exist.")
                .When(v => !string.IsNullOrEmpty(v.MessageData.ThreadId));

            // If InReplyToMessageId is provided, it must be a valid GUID and exist.
            RuleFor(v => v.MessageData.InReplyToMessageId)
                .Must(BeAValidGuid).WithMessage("InReplyToMessageId must be a valid GUID.")
                .When(v => !string.IsNullOrEmpty(v.MessageData.InReplyToMessageId))
                .MustAsync(MessageMustExist).WithMessage("The message you are replying to does not exist.")
                .When(v => !string.IsNullOrEmpty(v.MessageData.InReplyToMessageId));

            // If a PropertyId is provided, it must be a valid GUID and exist.
            RuleFor(v => v.MessageData.PropertyId)
                .Must(BeAValidGuid).WithMessage("PropertyId must be a valid GUID.")
                .When(v => !string.IsNullOrEmpty(v.MessageData.PropertyId))
                .MustAsync(PropertyMustExist).WithMessage("The specified property does not exist.")
                .When(v => !string.IsNullOrEmpty(v.MessageData.PropertyId));
        });
    }

    private bool BeAValidGuid(string? guidString)
    {
        return Guid.TryParse(guidString, out _);
    }

    private async Task<bool> RecipientMustExist(string? id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var guid))
            return false;
        return await _context.Members.AnyAsync(m => m.Id == guid, cancellationToken);
    }

    private async Task<bool> ThreadMustExist(string? id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var guid))
            return false;
        return await _context.MessageThreads.AnyAsync(t => t.Id == guid, cancellationToken);
    }

    private async Task<bool> MessageMustExist(string? id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var guid))
            return false;
        return await _context.Messages.AnyAsync(m => m.Id == guid, cancellationToken);
    }

    private async Task<bool> PropertyMustExist(string? id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var guid))
            return false;
        return await _context.EstateProperties.AnyAsync(p => p.Id == guid, cancellationToken);
    }
}
