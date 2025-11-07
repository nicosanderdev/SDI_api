using SDI_Api.Application.Common.Interfaces;
using NotFoundException = SDI_Api.Application.Common.Exceptions.NotFoundException;

namespace SDI_Api.Application.Messages.Commands;

public record SoftDeleteMessageCommand(Guid MessageId, Guid UserId) : IRequest;

public class SoftDeleteMessageCommandHandler : IRequestHandler<SoftDeleteMessageCommand>
{
    private readonly IApplicationDbContext _context;

    public SoftDeleteMessageCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(SoftDeleteMessageCommand request, CancellationToken cancellationToken)
    {
        var memberId = await _context.Members.Where(m => m.UserId == request.UserId)
            .Select(m => m.Id).FirstOrDefaultAsync(cancellationToken);
        if (memberId == Guid.Empty)
            throw new NotFoundException("Member not found for user.", request.UserId.ToString());

        var recipient = await _context.MessageRecipients
            .FirstOrDefaultAsync(mr => mr.MessageId == request.MessageId && mr.RecipientId == memberId, cancellationToken);
        if (recipient == null)
            throw new NotFoundException("Message not found for user.", request.MessageId.ToString());

        if (!recipient.IsDeleted)
        {
            recipient.IsDeleted = true;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}


