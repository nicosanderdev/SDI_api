using SDI_Api.Application.Common.Interfaces;

namespace SDI_Api.Application.Messages.Commands;

public class UpdateMessageRecipientStatusCommand : IRequest
{
    public Guid MessageRecipientId { get; init; }
    public Guid MessageId { get; init; }
}

public class UpdateMessageRecipientStatusCommandHanlder : IRequestHandler<UpdateMessageRecipientStatusCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateMessageRecipientStatusCommandHanlder(IApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task Handle(UpdateMessageRecipientStatusCommand request, CancellationToken cancellationToken)
    {
        var message = await _context.Messages.FirstOrDefaultAsync(m => 
            m.Id == request.MessageId && 
            m.MessageRecipients.Any(mr => 
                mr.RecipientId == request.MessageRecipientId), cancellationToken);
        
        if (message == null)
            throw new NotFoundException("Message not found.", request.MessageId.ToString());

        _context.Messages.Remove(message);
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            throw new DbUpdateException("An error occured while deleting message.", ex);
        }
    }
}
