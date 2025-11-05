using SDI_Api.Application.Common.Interfaces;
using Sdi_Api.Application.DTOs.Messages;
using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.Messages.Queries;

public class GetMessagesByPropertyIdQuery : IRequest<List<MessageDto>>
{
    public Guid PropertyId { get; }

    public GetMessagesByPropertyIdQuery(Guid propertyId)
    {
        PropertyId = propertyId;
    }
}

public class GetMessagesByPropertyIdQueryHandler : IRequestHandler<GetMessagesByPropertyIdQuery, List<MessageDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetMessagesByPropertyIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<MessageDto>> Handle(GetMessagesByPropertyIdQuery request, CancellationToken cancellationToken)
    {
        var messages = await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Thread)
                .ThenInclude(t => t.Property)
            .Include(m => m.MessageRecipients)
            .Where(m => m.Thread.PropertyId == request.PropertyId)
            .OrderByDescending(m => m.CreatedAtUtc)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var messageDtos = messages.Select(m => 
        {
            var dto = _mapper.Map<MessageDto>(m);
            // Set recipient info if available (using first recipient as default)
            var firstRecipient = m.MessageRecipients.FirstOrDefault();
            if (firstRecipient != null)
            {
                dto.RecipientId = firstRecipient.RecipientId.ToString();
                dto.IsRead = firstRecipient.IsRead;
                dto.IsReplied = firstRecipient.HasBeenRepliedToByRecipient;
                dto.IsStarred = firstRecipient.IsStarred;
                dto.IsArchived = firstRecipient.IsArchived;
            }
            return dto;
        }).ToList();

        return messageDtos;
    }
}

