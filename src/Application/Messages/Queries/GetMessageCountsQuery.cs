using SDI_Api.Application.Common.Interfaces;
using Sdi_Api.Application.DTOs.Messages;

namespace SDI_Api.Application.Messages.Queries;

public class GetMessageCountsQuery : IRequest<TabCountsDto> { }

public class GetMessageCountsQueryHandler : IRequestHandler<GetMessageCountsQuery, TabCountsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetMessageCountsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<TabCountsDto> Handle(GetMessageCountsQuery request, CancellationToken cancellationToken)
    {
        var currentUserIdString = _currentUserService.GetUserId();
        if (currentUserIdString == Guid.Empty)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var counts = new TabCountsDto();

        counts.Inbox = await _context.MessageRecipients
            .CountAsync(mr => mr.RecipientId == currentUserIdString && !mr.IsArchived && !mr.IsDeleted && !mr.IsRead, cancellationToken);
        
        counts.Starred = await _context.MessageRecipients
            .CountAsync(mr => mr.RecipientId == currentUserIdString && mr.IsStarred && !mr.IsDeleted, cancellationToken);

        counts.Replied = await _context.MessageRecipients
            .CountAsync(mr => mr.RecipientId == currentUserIdString && mr.HasBeenRepliedToByRecipient && !mr.IsArchived && !mr.IsDeleted, cancellationToken);
        
        counts.Archived = await _context.MessageRecipients
            .CountAsync(mr => mr.RecipientId == currentUserIdString && mr.IsArchived && !mr.IsDeleted, cancellationToken);
        
        counts.Trash = await _context.MessageRecipients
            .CountAsync(mr => mr.RecipientId == currentUserIdString && mr.IsDeleted, cancellationToken);

        counts.Sent = await _context.Messages
            .CountAsync(m => m.SenderId == currentUserIdString, cancellationToken); // Sent items might not have user-specific statuses like "IsDeleted" from sender's view.

        return counts;
    }
}
