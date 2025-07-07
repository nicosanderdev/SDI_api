using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.EstateProperties.Commands.Delete;

public class DeleteEstatePropertyCommand : IRequest
{
    public Guid Id { get; }
    public DeleteEstatePropertyCommand(Guid id) => Id = id;
}

public class DeleteEstatePropertyCommandHandler : IRequestHandler<DeleteEstatePropertyCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteEstatePropertyCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteEstatePropertyCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.EstateProperties
            .Include(p => p.PropertyImages)
            .Include(p => p.EstatePropertyValues)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (entity == null)
            throw new NotFoundException(nameof(EstateProperty), request.Id.ToString());
        
        foreach (var image in entity.PropertyImages)
            image.IsDeleted = true;
        foreach (var description in entity.EstatePropertyValues)
            description.IsDeleted = true;
        
        entity.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
