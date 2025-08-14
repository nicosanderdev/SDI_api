using SDI_Api.Application.Common.Exceptions;
using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Domain.Entities;
using NotFoundException = Ardalis.GuardClauses.NotFoundException;

namespace SDI_Api.Application.Messages.Commands;

public class MarkMessageAsReadCommand : IRequest
{
    public Guid? MessageId { get; }
    public Guid? UserId { get; }

    public MarkMessageAsReadCommand(Guid messageId, Guid userId)
    {
        MessageId = messageId;
        UserId = userId;
    }
}

public class MarkMessageAsReadCommandHandler : IRequestHandler<MarkMessageAsReadCommand>
{
    private readonly IApplicationDbContext _context;

    public MarkMessageAsReadCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(MarkMessageAsReadCommand request, CancellationToken cancellationToken)
    {
        var memberId = _context.Members.Where(m => m.UserId == request.UserId).Select(m => m.Id).FirstOrDefault();
        var memberIdString = memberId.ToString();
        
        var messageRecipient = await _context.MessageRecipients
            .FirstOrDefaultAsync(mr => mr.MessageId == request.MessageId && mr.RecipientId.ToString() == memberIdString, cancellationToken);

        if (messageRecipient == null)
        {
            var messageExists = await _context.Messages.AnyAsync(m => m.Id == request.MessageId, cancellationToken);
            if(!messageExists) 
                throw new NotFoundException(nameof(Message), request.MessageId.ToString()!);
            
            throw new ForbiddenAccessException();
        }

        if (!messageRecipient.IsRead)
        {
            messageRecipient.IsRead = true;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
