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
    [ProducesResponseType(typeof(PaginatedResult<PublicEstatePropertyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPublicProperties([FromQuery] GetEstatePropertiesQuery query)
    {
        var response = await _sender.Send(query);
        return Ok(response);
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
}
