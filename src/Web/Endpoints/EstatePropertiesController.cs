using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SDI_Api.Application.Common.Models;
using SDI_Api.Application.DTOs.EstateProperties;
using SDI_Api.Application.EstateProperties.Commands;
using SDI_Api.Application.EstateProperties.Commands.Create;
using SDI_Api.Application.EstateProperties.Commands.Delete;
using SDI_Api.Application.EstateProperties.Commands.Edit;
using SDI_Api.Application.EstateProperties.Queries;

namespace SDI_Api.Web.Endpoints;

[ApiController]
// [Authorize]
[Route("api/properties")]
public class EstatePropertiesController : ControllerBase
{
    private readonly ISender _sender;

    public EstatePropertiesController(ISender sender)
    {
        _sender = sender;
    }
    
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetEstateProperties([FromQuery] GetEstatePropertiesQuery query)
    {
        // Manually bind filter parameters from query string to support filter[property] notation
        BindFilterFromQueryString(query.Filter);
        
        var response = await _sender.Send(query);
        return Ok(response);
    }
    
    private void BindFilterFromQueryString(PropertyFilterDto filter)
    {
        // Try both bracket notation (filter[swLat]) and dot notation (filter.swLat) and direct (swLat)
        var query = Request.Query;
        
        // Helper to get value with multiple possible keys (case-insensitive)
        string? GetQueryValue(params string[] keys)
        {
            foreach (var key in keys)
            {
                // Try exact match first
                if (query.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
                    return value.ToString();
                    
                // Try case-insensitive match
                var matchingKey = query.Keys.FirstOrDefault(k => 
                    string.Equals(k, key, StringComparison.OrdinalIgnoreCase));
                if (matchingKey != null && query.TryGetValue(matchingKey, out value) && !string.IsNullOrWhiteSpace(value))
                    return value.ToString();
            }
            return null;
        }
        
        // Bind filter properties from query string
        if (GetQueryValue("filter[swLat]", "filter.swLat", "SwLat", "swLat") is { } swLatStr && 
            float.TryParse(swLatStr, out var swLat))
            filter.SwLat = swLat;
            
        if (GetQueryValue("filter[swLng]", "filter.swLng", "SwLng", "swLng") is { } swLngStr && 
            float.TryParse(swLngStr, out var swLng))
            filter.SwLng = swLng;
            
        if (GetQueryValue("filter[neLat]", "filter.neLat", "NeLat", "neLat") is { } neLatStr && 
            float.TryParse(neLatStr, out var neLat))
            filter.NeLat = neLat;
            
        if (GetQueryValue("filter[neLng]", "filter.neLng", "NeLng", "neLng") is { } neLngStr && 
            float.TryParse(neLngStr, out var neLng))
            filter.NeLng = neLng;
        
        if (GetQueryValue("filter[ownerId]", "filter.ownerId", "OwnerId", "ownerId") is { } ownerId)
            filter.OwnerId = ownerId;
            
        if (GetQueryValue("filter[status]", "filter.status", "Status", "status") is { } status)
            filter.Status = status;
            
        if (GetQueryValue("filter[searchTerm]", "filter.searchTerm", "SearchTerm", "searchTerm") is { } searchTerm)
            filter.SearchTerm = searchTerm;
        
        if (GetQueryValue("filter[createdAfter]", "filter.createdAfter", "CreatedAfter", "createdAfter") is { } createdAfterStr && 
            DateTime.TryParse(createdAfterStr, out var createdAfter))
            filter.CreatedAfter = createdAfter;
            
        if (GetQueryValue("filter[createdBefore]", "filter.createdBefore", "CreatedBefore", "createdBefore") is { } createdBeforeStr && 
            DateTime.TryParse(createdBeforeStr, out var createdBefore))
            filter.CreatedBefore = createdBefore;
        
        if (GetQueryValue("filter[includeImages]", "filter.includeImages", "IncludeImages", "includeImages") is { } includeImagesStr && 
            bool.TryParse(includeImagesStr, out var includeImages))
            filter.IncludeImages = includeImages;
            
        if (GetQueryValue("filter[includeVideos]", "filter.includeVideos", "IncludeVideos", "includeVideos") is { } includeVideosStr && 
            bool.TryParse(includeVideosStr, out var includeVideos))
            filter.IncludeVideos = includeVideos;
            
        if (GetQueryValue("filter[includeDocuments]", "filter.includeDocuments", "IncludeDocuments", "includeDocuments") is { } includeDocumentsStr && 
            bool.TryParse(includeDocumentsStr, out var includeDocuments))
            filter.IncludeDocuments = includeDocuments;
            
        if (GetQueryValue("filter[includeAmenities]", "filter.includeAmenities", "IncludeAmenities", "includeAmenities") is { } includeAmenitiesStr && 
            bool.TryParse(includeAmenitiesStr, out var includeAmenities))
            filter.IncludeAmenities = includeAmenities;
            
        // For users endpoint - IsDeleted flag
        if (GetQueryValue("filter[isDeleted]", "filter.isDeleted", "IsDeleted", "isDeleted") is { } isDeletedStr && 
            bool.TryParse(isDeletedStr, out var isDeleted))
            filter.IsDeleted = isDeleted;
    }
    
    [HttpGet("mine")]
    [ProducesResponseType(typeof(PaginatedResult<UsersEstatePropertyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUsersProperties([FromQuery] GetUsersEstatePropertiesQuery query)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdValue))
            throw new UnauthorizedAccessException("User identifier not found.");

        if (!Guid.TryParse(userIdValue, out var userGuid))
            throw new ArgumentException("Invalid user identifier format.");

        // Manually bind filter parameters from query string to support filter[property] notation
        BindFilterFromQueryString(query.Filter);
        
        query.UserId = userGuid;
        var response = await _sender.Send(query);
        return Ok(response);
    }

    [HttpPost("create")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateEstateProperty([FromForm] CreateOrUpdateEstatePropertyDto request)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdValue == null)
            throw new UnauthorizedAccessException("User identifier not found.");
        
        if (!string.IsNullOrEmpty(request.LocationString)) 
            request.Location = JsonSerializer.Deserialize<LocationDto>(request.LocationString);
        
        var command = new CreateEstatePropertyCommand { CreateOrUpdateEstatePropertyDto = request };
        command.CreateOrUpdateEstatePropertyDto!.OwnerId = userIdValue;
        var createdPropertyDto = await _sender.Send(command);
        return Created(nameof(CreateOrUpdateEstatePropertyDto), createdPropertyDto);
    }

    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateEstateProperty([FromRoute] string id, [FromForm] CreateOrUpdateEstatePropertyDto request)
    {
        if (!Guid.TryParse(id, out var guidId))
            throw new ArgumentException("Invalid ID format.");
        
        if (request.Id == Guid.Empty)
            request.Id = guidId;
        else if (request.Id != guidId) 
            throw new ArgumentException("Mismatched ID in route and body.");

        var command = new UpdateEstatePropertyCommand();
        command.EstatePropertyDto = request;
        await _sender.Send(command);
        var updatedPropertyDto = await _sender.Send(new GetEstatePropertyByIdQuery(guidId));
        return Ok(updatedPropertyDto);
    }

    [HttpPost("{id:guid}/duplicate")]
    [ProducesResponseType(typeof(DuplicatedEstatePropertyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DuplicateProperty(Guid id)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdValue))
            throw new UnauthorizedAccessException("User identifier not found.");

        if (!Guid.TryParse(userIdValue, out var userGuid))
            throw new ArgumentException("Invalid user identifier format.");
        
        var command = new DuplicateEstatePropertyCommand
        {
            OriginalPropertyId = id,
            UserId = userGuid
        };
        
        var response = await _sender.Send(command);
        return Ok(response);
    }
    
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteEstateProperty([FromRoute] string id)
    {
        if (!Guid.TryParse(id, out var guidId))
            throw new ArgumentException("Invalid ID format.");
        
        await _sender.Send(new DeleteEstatePropertyCommand(guidId));
        return NoContent();
    }

    [HttpGet("amenities")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllAmenities()
    {
        var result = await _sender.Send(new GetAllAmenitiesQuery());
        return Ok(result);
    }

    [HttpGet("favorites")]
    [ProducesResponseType(typeof(ICollection<PropertyAsFavoriteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPropertiesAsFavorite()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdValue))
            throw new UnauthorizedAccessException("User identifier not found.");

        var request = new GetUserFavoritePropertiesQuery();
        request.UserId = Guid.Parse(userIdValue);
        var response = await _sender.Send(request);

        return Ok(response);
    }

    [HttpPost("favorite-update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddPropertyAsFavorite([FromBody] UpdateEstatePropertiesFavoritesCommand command)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdValue))
            throw new UnauthorizedAccessException("User identifier not found.");

        if (!Guid.TryParse(userIdValue, out var userGuid))
            throw new ArgumentException("Invalid user identifier format.");
        command.FavoriteDto.UserId = userGuid;
        
        var response = await _sender.Send(command);
        return Ok(response);
    }
}
