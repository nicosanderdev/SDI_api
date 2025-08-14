using SDI_Api.Application.Common.Exceptions;
using SDI_Api.Application.Common.Interfaces;
using Sdi_Api.Application.DTOs.Messages;
using SDI_Api.Application.Messages.Queries;
using SDI_Api.Domain.Entities;
using NotFoundException = Ardalis.GuardClauses.NotFoundException;

namespace SDI_Api.Application.Messages.Queries;

public class GetMessageByIdQuery : IRequest<MessageDetailDto>
{
    public Guid MessageId { get; }
    public Guid UserId { get; set; }

    public GetMessageByIdQuery(Guid messageId, Guid userId)
    {
        MessageId = messageId;
        UserId = userId;
    }
}

public class GetMessageByIdQueryHandler : IRequestHandler<GetMessageByIdQuery, MessageDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetMessageByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<MessageDetailDto> Handle(GetMessageByIdQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
            throw new UnauthorizedAccessException("User is not authenticated.");
        var memberId = _context.Members.Where(m => m.UserId == request.UserId).Select(m => m.Id).FirstOrDefault();

        var message = await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Thread)
                .ThenInclude(t => t.Property)
            .Include(m => m.MessageRecipients)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == request.MessageId, cancellationToken);

        if (message == null)
            throw new NotFoundException(nameof(Message), request.MessageId.ToString());

        // Check if current user is either the sender or one of the recipients
        var userRecipientEntry = message.MessageRecipients.FirstOrDefault(mr => mr.RecipientId == memberId);
        bool isSender = message.SenderId == memberId;

        if (!isSender && userRecipientEntry == null)
            throw new ForbiddenAccessException(); //[$"User does not have access to message {request.MessageId}."]

        var dto = _mapper.Map<MessageDetailDto>(message);

        // If the current user is a recipient, set their specific flags
        if (userRecipientEntry != null)
        {
            dto.RecipientId = userRecipientEntry.RecipientId.ToString();
            dto.IsRead = userRecipientEntry.IsRead;
            dto.IsReplied = userRecipientEntry.HasBeenRepliedToByRecipient;
            dto.IsStarred = userRecipientEntry.IsStarred;
            dto.IsArchived = userRecipientEntry.IsArchived;

            // Optionally, mark as read if viewing details and user is recipient
            // This logic is typically in a separate MarkAsRead command triggered by UI
        }
        else if (isSender) // If current user is the sender
        {
            // For sender, these flags might not be directly applicable in the same way
            // Or they could reflect the status of the primary recipient if desired (more complex)
            dto.RecipientId = null; // Sender is not a "recipient" of their own message
            dto.IsRead = true; // Sender has "read" their own message
            dto.IsReplied = message.MessageRecipients.Any(mr => mr.HasBeenRepliedToByRecipient); // If any recipient replied
            dto.IsStarred = false; // Sender doesn't star their own sent message typically
            dto.IsArchived = false;
        }
        
        return dto;
    }
}
