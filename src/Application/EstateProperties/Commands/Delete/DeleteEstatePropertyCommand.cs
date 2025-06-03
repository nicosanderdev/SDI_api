using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.EstateProperties.Commands.Delete;

public class DeleteEstatePropertyCommand : IRequest<Unit>
{
    public Guid Id { get; }
    public DeleteEstatePropertyCommand(Guid id) => Id = id;
}

public class DeleteEstatePropertyCommandHandler : IRequestHandler<DeleteEstatePropertyCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteEstatePropertyCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteEstatePropertyCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.EstateProperties
            .Include(p => p.PropertyImages) // Include related entities for cascade delete if configured, or manual removal
            .Include(p => p.EstatePropertyDescriptions)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(EstateProperty), request.Id.ToString());
        }

        // EF Core might handle cascade deletes if configured.
        // Otherwise, remove dependents manually or ensure DB cascade rules.
        // For this example, assuming cascade delete is handled by DB or EF Core relationship configuration.
        // If not, you'd do:
        // _context.PropertyImages.RemoveRange(entity.PropertyImages);
        // _context.EstatePropertyDescriptions.RemoveRange(entity.EstatePropertyDescriptions);

        _context.EstateProperties.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
