using SDI_Api.Application.Common.Interfaces;
using NotFoundException = SDI_Api.Application.Common.Exceptions.NotFoundException;

namespace SDI_Api.Application.Messages.Commands;

public class DeleteMessageCommand : IRequest
{
    public Guid Id { get; init; }
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
        var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);
        if (message == null)
            throw new NotFoundException("Message not found.", request.Id.ToString());

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
