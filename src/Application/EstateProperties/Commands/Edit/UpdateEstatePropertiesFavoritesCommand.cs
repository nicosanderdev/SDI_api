using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.DTOs.EstateProperties;
using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.EstateProperties.Commands.Edit;

public class UpdateEstatePropertiesFavoritesCommand : IRequest<Unit>
{
    public PropertyAsFavoriteDto FavoriteDto { get; set; } = new PropertyAsFavoriteDto();
}

public class UpdateEstatePropertiesFavoritesCommandHandler : IRequestHandler<UpdateEstatePropertiesFavoritesCommand, Unit>
{
    IApplicationDbContext _context;
    
    public UpdateEstatePropertiesFavoritesCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<Unit> Handle(UpdateEstatePropertiesFavoritesCommand request, CancellationToken cancellationToken)
    {
        var dto = request.FavoriteDto;
        
        var memberId = await _context.Members
            .AsNoTracking()
            .Where(m => m.UserId == dto!.UserId)
            .Select(m => m.Id)
            .SingleOrDefaultAsync(cancellationToken);
        
        if (!dto.IsFavorite)
        {
            var favorite = await _context.Favorites.FirstOrDefaultAsync(f => f.MemberId == memberId && f.EstatePropertyId == dto.EstatePropertyId, cancellationToken);
            if (favorite != null)
                _context.Favorites.Remove(favorite);
        
            await _context.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }

        _context.Favorites.Add(new Favorite()
        {
            MemberId = memberId, EstatePropertyId = dto!.EstatePropertyId, FavoritedAt = new DateTimeOffset()
        });
        
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
