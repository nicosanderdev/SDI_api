using SDI_Api.Application.Common.Interfaces;
using NotFoundException = SDI_Api.Application.Common.Exceptions.NotFoundException;

namespace SDI_Api.Application.Messages.Commands;

public record StarMessageCommand(Guid MessageId, Guid UserId) : IRequest;

public class StarMessageCommandHandler : IRequestHandler<StarMessageCommand>
{
    private readonly IApplicationDbContext _context;

    public StarMessageCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(StarMessageCommand request, CancellationToken cancellationToken)
    {
        var memberId = await _context.Members.Where(m => m.UserId == request.UserId)
            .Select(m => m.Id).FirstOrDefaultAsync(cancellationToken);
        if (memberId == Guid.Empty)
            throw new NotFoundException("Member not found for user.", request.UserId.ToString());

        var recipient = await _context.MessageRecipients
            .FirstOrDefaultAsync(mr => mr.MessageId == request.MessageId && mr.RecipientId == memberId, cancellationToken);
        if (recipient == null)
            throw new NotFoundException("Message not found for user.", request.MessageId.ToString());

        if (!recipient.IsStarred)
        {
            recipient.IsStarred = true;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}


