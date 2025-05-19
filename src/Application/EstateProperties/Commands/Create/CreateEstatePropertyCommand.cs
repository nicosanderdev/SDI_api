using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.Common.Models;
using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.EstateProperties.Commands.Create;

public record CreateEstatePropertyCommand(CreateEstatePropertyRequest Request) : IRequest;

public class CreateEstatePropertyCommandHandler : IRequestHandler<CreateEstatePropertyCommand>
{
    private readonly IApplicationDbContext _context;

    public CreateEstatePropertyCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(CreateEstatePropertyCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        
        var estateProperty = new EstateProperty
        {
            EstatePropertyDescriptions = new(),
            FeaturedDescription = null,
            FeaturedDescriptionId = null
        };

        _context.EstateProperties.Add(estateProperty);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
