using SDI_Api.Application.Common.Exceptions;
using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Domain.Entities;
using NotFoundException = Ardalis.GuardClauses.NotFoundException;

namespace SDI_Api.Application.Messages.Commands;

public class MarkMessageAsReadCommand : IRequest<Unit> // Using Unit for no specific return value
{
    public Guid MessageId { get; }

    public MarkMessageAsReadCommand(Guid messageId)
    {
        MessageId = messageId;
    }
}

public class MarkMessageAsReadCommandHandler : IRequestHandler<MarkMessageAsReadCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public MarkMessageAsReadCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(MarkMessageAsReadCommand request, CancellationToken cancellationToken)
    {
        var currentUserIdString = _currentUserService.GetUserId().ToString();
         if (string.IsNullOrEmpty(currentUserIdString))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var messageRecipient = await _context.MessageRecipients
            .FirstOrDefaultAsync(mr => mr.MessageId == request.MessageId && mr.RecipientId.ToString() == currentUserIdString, cancellationToken);

        if (messageRecipient == null)
        {
            // Could be that the message doesn't exist, or user is not a recipient.
            // Check if message exists at all, to differentiate.
            var messageExists = await _context.Messages.AnyAsync(m => m.Id == request.MessageId, cancellationToken);
            if(!messageExists) throw new NotFoundException(nameof(Message), request.MessageId.ToString());
            
            throw new ForbiddenAccessException(); //["User is not a recipient of message {request.MessageId} or message does not exist for user."]
        }

        if (!messageRecipient.IsRead)
        {
            messageRecipient.IsRead = true;
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
