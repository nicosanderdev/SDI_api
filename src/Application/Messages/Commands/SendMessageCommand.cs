using SDI_Api.Application.Common.Interfaces;
using Sdi_Api.Application.DTOs.Messages;
using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.Messages.Commands;

public class SendMessageCommand : IRequest<MessageDto>
{
    public SendMessageDto MessageData { get; init; } = null!;
}

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, MessageDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IIdentityService _identityService; // To find recipient user

    public SendMessageCommandHandler(IApplicationDbContext context, IMapper mapper, ICurrentUserService currentUserService, IIdentityService identityService)
    {
        _context = context;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _identityService = identityService;
    }

    public async Task<MessageDto> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var senderUserIdString = _currentUserService.GetUserId().ToString();
        if (string.IsNullOrEmpty(senderUserIdString))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var sender = await _identityService.FindUserByIdAsync(senderUserIdString); // Fetches ApplicationUser
        if (sender == null) throw new NotFoundException(nameof(IUser), senderUserIdString);
        // Ensure sender.Member is loaded if needed for mapping, though FindUserByIdAsync in IIdentityService can include it.
        // For the sender info in the DTO, we'll rely on mapping from `sender` object later.
        
        var recipient = await _identityService.FindUserByIdAsync(request.MessageData.RecipientId);
        if (recipient == null) throw new NotFoundException(nameof(IUser), request.MessageData.RecipientId);


        MessageThread thread;
        Message? repliedToMessage = null;

        if (!string.IsNullOrEmpty(request.MessageData.ThreadId) && Guid.TryParse(request.MessageData.ThreadId, out var existingThreadGuid))
        {
            thread = await _context.MessageThreads.FindAsync([existingThreadGuid.ToString()], cancellationToken)
                ?? throw new NotFoundException(nameof(MessageThread), existingThreadGuid.ToString());
        }
        else if (!string.IsNullOrEmpty(request.MessageData.InReplyToMessageId) && Guid.TryParse(request.MessageData.InReplyToMessageId, out var inReplyToMsgGuid))
        {
            repliedToMessage = await _context.Messages
                .Include(m => m.Thread)
                .FirstOrDefaultAsync(m => m.Id == inReplyToMsgGuid, cancellationToken)
                ?? throw new NotFoundException(nameof(Message), inReplyToMsgGuid.ToString());
            thread = repliedToMessage.Thread;

            MessageRecipient? repliedToMessageRecipientStatus = await _context.MessageRecipients
                .FirstOrDefaultAsync(mr => mr.MessageId == inReplyToMsgGuid && mr.RecipientId.ToString() == senderUserIdString, cancellationToken);
            if (repliedToMessageRecipientStatus != null)
            {
                repliedToMessageRecipientStatus.HasBeenRepliedToByRecipient = true;
            }
        }
        else
        {
            Guid? propertyGuid = null;
            if (!string.IsNullOrEmpty(request.MessageData.PropertyId) && Guid.TryParse(request.MessageData.PropertyId, out var pGuid))
            {
                if (!await _context.EstateProperties.AnyAsync(ep => ep.Id == pGuid, cancellationToken))
                    throw new NotFoundException(nameof(EstateProperty), pGuid.ToString());
                propertyGuid = pGuid;
            }
            thread = new MessageThread { Subject = request.MessageData.Subject, PropertyId = propertyGuid, CreatedAtUtc = DateTime.UtcNow, LastMessageAtUtc = DateTime.UtcNow };
            _context.MessageThreads.Add(thread);
        }

        var message = new Message
        {
            ThreadId = thread.Id,
            SenderId = Guid.Parse(senderUserIdString),
            Body = request.MessageData.Body,
            Snippet = GenerateSnippet(request.MessageData.Body, 150),
            CreatedAtUtc = DateTime.UtcNow,
            InReplyToMessageId = repliedToMessage?.Id
        };
        _context.Messages.Add(message);
        thread.LastMessageAtUtc = message.CreatedAtUtc;

        var messageRecipientEntry = new MessageRecipient()
        {
            MessageId = message.Id,
            RecipientId = Guid.Parse(request.MessageData.RecipientId),
            ReceivedAtUtc = DateTime.UtcNow,
        };
        _context.MessageRecipients.Add(messageRecipientEntry);
        
        await _context.SaveChangesAsync(cancellationToken);

        // Reload the message with necessary includes for mapping the DTO accurately
        var createdMessageWithIncludes = await _context.Messages
            //.Include(s => s.Member) // For sender name
            .Include(m => m.Thread).ThenInclude(t => t.Property)
            .AsNoTracking()
            .FirstAsync(m => m.Id == message.Id, cancellationToken);

        var dto = _mapper.Map<MessageDto>(createdMessageWithIncludes);
        dto.RecipientId = request.MessageData.RecipientId; // Set who the message was for
        // For the sender's immediate response, these flags are from recipient's perspective.
        dto.IsRead = false; 
        dto.IsReplied = false;
        dto.IsStarred = false;
        dto.IsArchived = false;

        return dto;
    }

    private string GenerateSnippet(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        return text.Length <= maxLength ? text : text.Substring(0, maxLength) + "...";
    }
}
