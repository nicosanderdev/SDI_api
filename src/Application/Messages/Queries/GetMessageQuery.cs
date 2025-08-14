using SDI_Api.Application.Common.Interfaces;
using Sdi_Api.Application.DTOs.Messages;
using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.Messages.Queries;

public class GetMessagesQuery : IRequest<PaginatedMessageResultDto>
{
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 15;
    public string? Filter { get; set; }
    public string? Query { get; set; }
    public string? PropertyId { get; set; }
    public string? SortBy { get; set; } = "createdAt_desc";
    public Guid? UserId { get; set; }
}

public class GetMessagesQueryHandler : IRequestHandler<GetMessagesQuery, PaginatedMessageResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetMessagesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedMessageResultDto> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
            throw new UnauthorizedAccessException("User is not authenticated.");
        var memberId = _context.Members.Where(m => m.UserId == request.UserId).Select(m => m.Id).FirstOrDefault();

        IQueryable<MessageRecipient> query = _context.MessageRecipients
            .Include(mr => mr.Message)
                .ThenInclude(m => m.Sender)
                    // .ThenInclude(s => s.Member)
            .Include(mr => mr.Message)
                .ThenInclude(m => m.Thread)
                    .ThenInclude(t => t.Property)
            .Where(mr => mr.RecipientId == memberId);

        // Apply Filters
        request.Filter = request.Filter?.ToLowerInvariant() ?? "inbox";
        switch (request.Filter)
        {
            case "inbox":
                query = query.Where(mr => !mr.IsArchived && !mr.IsDeleted);
                break;
            case "starred":
                query = query.Where(mr => mr.IsStarred && !mr.IsDeleted);
                break;
            case "replied":
                 query = query.Where(mr => mr.HasBeenRepliedToByRecipient && !mr.IsArchived && !mr.IsDeleted);
                break;
            case "archived":
                query = query.Where(mr => mr.IsArchived && !mr.IsDeleted);
                break;
            case "sent":
                var sentMessagesQuery = _context.Messages
                    .Include(m => m.Sender)
                        //.ThenInclude(s => s.Member) // For Sender info
                    .Include(m => m.Thread).ThenInclude(t => t.Property)
                    .Include(m => m.MessageRecipients).ThenInclude(mr => mr.Recipient)
                        // .ThenInclude(r => r.Member) // For Recipient info display
                    .Where(m => m.SenderId == memberId)
                    .Select(m => new MessageDto
                    {
                        Id = m.Id.ToString(),
                        ThreadId = m.ThreadId.ToString(),
                        SenderId = m.SenderId.ToString(),
                        SenderName = m.Sender.LastName != null ? $"{m.Sender.FirstName} {m.Sender.LastName}".Trim() : m.Sender.FirstName!,
                        // SenderEmail = m.Sender.Email,
                        RecipientId = m.MessageRecipients.FirstOrDefault() != null ? m.MessageRecipients.First().RecipientId.ToString() : null,
                        // If you want recipient name for sent items:
                        // RecipientName = m.MessageRecipients.FirstOrDefault() != null && m.MessageRecipients.First().Recipient.Member != null
                        //                ? $"{m.MessageRecipients.First().Recipient.Member.FirstName} {m.MessageRecipients.First().Recipient.Member.LastName}".Trim()
                        //                : (m.MessageRecipients.FirstOrDefault() != null ? m.MessageRecipients.First().Recipient.UserName : null),
                        PropertyId = m.Thread.PropertyId.HasValue ? m.Thread.PropertyId.Value.ToString() : null,
                        PropertyTitle = m.Thread.Property != null ? m.Thread.Property.Title : null,
                        Subject = m.Thread.Subject,
                        Snippet = m.Snippet,
                        CreatedAt = m.CreatedAtUtc.ToString("o"),
                        IsRead = true, IsReplied = false, IsStarred = false, IsArchived = false, // Defaults for sent items view
                    });
                
                if (!string.IsNullOrWhiteSpace(request.Query))
                {
                    sentMessagesQuery = sentMessagesQuery.Where(m => m.Subject.Contains(request.Query) || m.Snippet.Contains(request.Query) || m.SenderName.Contains(request.Query));
                }
                if (Guid.TryParse(request.PropertyId, out var sentPropGuid)) // PropertyId in MessageDto is string
                {
                    sentMessagesQuery = sentMessagesQuery.Where(m => m.PropertyId == sentPropGuid.ToString());
                }
                 sentMessagesQuery = request.SortBy switch { "createdAt_asc" => sentMessagesQuery.OrderBy(m => m.CreatedAt), _ => sentMessagesQuery.OrderByDescending(m => m.CreatedAt), };
                var totalSent = await sentMessagesQuery.CountAsync(cancellationToken);
                var sentItems = await sentMessagesQuery.Skip((request.Page - 1) * request.Limit).Take(request.Limit).ToListAsync(cancellationToken);
                return new PaginatedMessageResultDto { Data = sentItems, Total = totalSent, Page = request.Page, TotalPages = (int)Math.Ceiling(totalSent / (double)request.Limit) };

            case "trash":
                query = query.Where(mr => mr.IsDeleted);
                break;
            default: 
                query = query.Where(mr => !mr.IsArchived && !mr.IsDeleted);
                break;
        }

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            query = query.Where(mr =>
                mr.Message.Thread.Subject.Contains(request.Query) ||
                mr.Message.Snippet.Contains(request.Query) ||
                (mr.Message.Sender.FirstName + " " + mr.Message.Sender.LastName).Contains(request.Query));
        }

        if (Guid.TryParse(request.PropertyId, out var propertyGuid))
            query = query.Where(mr => mr.Message.Thread.PropertyId == propertyGuid);

        query = request.SortBy switch { "createdAt_asc" => query.OrderBy(mr => mr.Message.CreatedAtUtc), _ => query.OrderByDescending(mr => mr.Message.CreatedAtUtc), };
        
        var totalCount = await query.CountAsync(cancellationToken);
        var messageRecipients = await query
            .Skip((request.Page - 1) * request.Limit)
            .Take(request.Limit)
            .ToListAsync(cancellationToken);

        var messageDtos = messageRecipients.Select(mr => {
            var dto = _mapper.Map<MessageDto>(mr.Message);
            dto.Id = mr.MessageId.ToString(); 
            dto.RecipientId = mr.RecipientId.ToString();
            dto.IsRead = mr.IsRead;
            dto.IsReplied = mr.HasBeenRepliedToByRecipient;
            dto.IsStarred = mr.IsStarred;
            dto.IsArchived = mr.IsArchived;
            return dto;
        }).ToList();

        return new PaginatedMessageResultDto() { Data = messageDtos, Total = totalCount, Page = request.Page, TotalPages = (int)Math.Ceiling(totalCount / (double)request.Limit) };
    }
}
