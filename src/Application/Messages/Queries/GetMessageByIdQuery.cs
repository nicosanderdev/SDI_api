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

    public GetMessageByIdQuery(Guid messageId)
    {
        MessageId = messageId;
    }
}

public class GetMessageByIdQueryHandler : IRequestHandler<GetMessageByIdQuery, MessageDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetMessageByIdQueryHandler(IApplicationDbContext context, IMapper mapper, ICurrentUserService currentUserService)
    {
        _context = context;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<MessageDetailDto> Handle(GetMessageByIdQuery request, CancellationToken cancellationToken)
    {
        var currentUserIdString = _currentUserService.GetUserId();
         if (currentUserIdString == Guid.Empty)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var message = await _context.Messages
            //.Include(m => m.Sender).ThenInclude(s => s.Member) // Include Member for Sender's profile info
            .Include(m => m.Thread).ThenInclude(t => t.Property)
            .Include(m => m.MessageRecipients) // To check if current user is a recipient or sender
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == request.MessageId, cancellationToken);

        if (message == null)
        {
            throw new NotFoundException(nameof(Message), request.MessageId.ToString());
        }

        // Check if current user is either the sender or one of the recipients
        var userRecipientEntry = message.MessageRecipients.FirstOrDefault(mr => mr.RecipientId == currentUserIdString);
        bool isSender = message.SenderId == currentUserIdString;

        if (!isSender && userRecipientEntry == null)
        {
            throw new ForbiddenAccessException(); //[$"User does not have access to message {request.MessageId}."]
        }

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
