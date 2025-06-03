using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.EstateProperties.Commands.SubmitQandAMessage;

public sealed record SubmitQandAMessageCommand(SubmitQandAMessageRequest request) : IRequest;

public class SubmitQandAMessageCommandHandler : IRequestHandler<SubmitQandAMessageCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public SubmitQandAMessageCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task Handle(SubmitQandAMessageCommand command, CancellationToken cancellationToken)
    {
        var request = command.request;

        QandAMessageThread? messageThread;

        // Create thread for new message.
        if (request.MessageThreadId != null)
        {
            messageThread = await _context.QandAMessageThreads
                .Include(q => q.Messages)
                .FirstOrDefaultAsync(q => q.Id == request.MessageThreadId.Value, cancellationToken);

            if (messageThread is null)
            {
                throw new NotFoundException("MessageThread not found", request.MessageThreadId.ToString()!);
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(request.Title) || request.EstatePropertyId == null)
            {
                throw new ValidationException("New threads require a Title and EstatePropertyId.");
            }

            var estateProperty = await _context.EstateProperties
                .FirstOrDefaultAsync(e => e.Id == request.EstatePropertyId.Value, cancellationToken);
            
            if (estateProperty is null)
            {
                throw new NotFoundException("EstateProperty not found", request.EstatePropertyId.ToString()!);
            }
            
            messageThread = new QandAMessageThread
            {
                Title = request.Title,
                EstatePropertyId = request.EstatePropertyId.Value,
                EstateProperty = estateProperty,
                Messages = new List<QandAMessage>()
            };

            _context.QandAMessageThreads.Add(messageThread);
        }
        
        var message = _mapper.Map<QandAMessage>(request);
        message.SentAt = DateTime.UtcNow;
        messageThread.Messages.Add(message);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
