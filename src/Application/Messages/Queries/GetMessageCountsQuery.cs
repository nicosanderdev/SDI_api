using SDI_Api.Application.Common.Interfaces;
using Sdi_Api.Application.DTOs.Messages;

namespace SDI_Api.Application.Messages.Queries;

public class GetMessageCountsQuery : IRequest<TabCountsDto> {
    public Guid? UserId { get; set; }
 }

public class GetMessageCountsQueryHandler : IRequestHandler<GetMessageCountsQuery, TabCountsDto>
{
    private readonly IApplicationDbContext _context;

    public GetMessageCountsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
    }

    public async Task<TabCountsDto> Handle(GetMessageCountsQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId == null || request.UserId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }
        var currentUserId = request.UserId!.Value;
        var counts = new TabCountsDto();

        counts.Inbox = await _context.MessageRecipients
            .CountAsync(mr => mr.RecipientId == currentUserId && !mr.IsArchived && !mr.IsDeleted && !mr.IsRead, cancellationToken);
        
        counts.Starred = await _context.MessageRecipients
            .CountAsync(mr => mr.RecipientId == currentUserId && mr.IsStarred && !mr.IsDeleted, cancellationToken);

        counts.Replied = await _context.MessageRecipients
            .CountAsync(mr => mr.RecipientId == currentUserId && mr.HasBeenRepliedToByRecipient && !mr.IsArchived && !mr.IsDeleted, cancellationToken);
        
        counts.Archived = await _context.MessageRecipients
            .CountAsync(mr => mr.RecipientId == currentUserId && mr.IsArchived && !mr.IsDeleted, cancellationToken);
        
        counts.Trash = await _context.MessageRecipients
            .CountAsync(mr => mr.RecipientId == currentUserId && mr.IsDeleted, cancellationToken);

        counts.Sent = await _context.Messages
            .CountAsync(m => m.SenderId == currentUserId, cancellationToken);

        return counts;
    }
}
