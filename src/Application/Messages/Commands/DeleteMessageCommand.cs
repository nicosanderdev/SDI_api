using SDI_Api.Application.Common.Interfaces;
using NotFoundException = SDI_Api.Application.Common.Exceptions.NotFoundException;

namespace SDI_Api.Application.Messages.Commands;

public class DeleteMessageCommand : IRequest
{
    public Guid? MessageId { get; }
    
    public DeleteMessageCommand(Guid messageId)
    {
        MessageId = messageId;
    }
}

public class DeleteMessageCommandHandler : IRequestHandler<DeleteMessageCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteMessageCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task Handle(DeleteMessageCommand request, CancellationToken cancellationToken)
    {
        var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == request.MessageId, cancellationToken);
        if (message == null)
            throw new NotFoundException("Message not found.", request.MessageId.ToString()!);

        _context.Messages.Remove(message);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
