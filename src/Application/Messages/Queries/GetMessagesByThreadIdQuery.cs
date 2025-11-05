using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.Common.Exceptions;
using Sdi_Api.Application.DTOs.Messages;
using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.Messages.Queries;

public class GetMessagesByThreadIdQuery : IRequest<List<MessageDetailDto>>
{
    public Guid ThreadId { get; }
    public Guid UserId { get; set; }
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 100;
    public string? SortBy { get; set; } = "createdAt_asc";

    public GetMessagesByThreadIdQuery(Guid threadId)
    {
        ThreadId = threadId;
    }
}

public class GetMessagesByThreadIdQueryHandler : IRequestHandler<GetMessagesByThreadIdQuery, List<MessageDetailDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetMessagesByThreadIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<MessageDetailDto>> Handle(GetMessagesByThreadIdQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var memberId = _context.Members
            .Where(m => m.UserId == request.UserId)
            .Select(m => m.Id)
            .FirstOrDefault();

        // Base query: all messages in the thread, with necessary includes
        var query = _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Thread)
                .ThenInclude(t => t.Property)
            .Include(m => m.MessageRecipients)
            .AsNoTracking()
            .Where(m => m.ThreadId == request.ThreadId);

        // Ensure user has access: either sender of any message in thread or recipient in thread
        var userHasAccess = await query
            .AnyAsync(m => m.SenderId == memberId || m.MessageRecipients.Any(mr => mr.RecipientId == memberId), cancellationToken);

        if (!userHasAccess)
            throw new ForbiddenAccessException();

        // Sorting
        query = request.SortBy switch
        {
            "createdAt_desc" => query.OrderByDescending(m => m.CreatedAtUtc),
            _ => query.OrderBy(m => m.CreatedAtUtc)
        };

        // Pagination
        if (request.Page < 1) request.Page = 1;
        if (request.Limit < 1) request.Limit = 100;

        var messages = await query
            .Skip((request.Page - 1) * request.Limit)
            .Take(request.Limit)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
            throw new Ardalis.GuardClauses.NotFoundException(nameof(Message), request.ThreadId.ToString());

        // Map to DTOs and apply recipient-specific flags for the current user when applicable
        var results = new List<MessageDetailDto>(messages.Count);

        foreach (var message in messages)
        {
            var asDetail = _mapper.Map<MessageDetailDto>(message);

            var recipientEntry = message.MessageRecipients.FirstOrDefault(mr => mr.RecipientId == memberId);
            if (recipientEntry != null)
            {
                asDetail.RecipientId = recipientEntry.RecipientId.ToString();
                asDetail.IsRead = recipientEntry.IsRead;
                asDetail.IsReplied = recipientEntry.HasBeenRepliedToByRecipient;
                asDetail.IsStarred = recipientEntry.IsStarred;
                asDetail.IsArchived = recipientEntry.IsArchived;
            }

            results.Add(asDetail);
        }

        return results;
    }
}


