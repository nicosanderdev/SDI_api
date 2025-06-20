using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SDI_Api.Application.EstateProperties.Commands.Create;
using SDI_Api.Application.EstateProperties.Commands.Delete;
using SDI_Api.Application.EstateProperties.Commands.Edit;
using SDI_Api.Application.EstateProperties.Queries;

namespace SDI_Api.Web.Endpoints;

[Route("api/properties")]
[ApiController]
public class EstatePropertiesController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IMapper _mapper;

    public EstatePropertiesController(ISender sender, IMapper mapper)
    {
        _sender = sender;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetEstateProperties([FromQuery] GetEstatePropertiesQuery query)
    {
        try
        {
            var response = await _sender.Send(query);
            return Ok(response); 
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error fetching properties.", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetEstateProperty([FromRoute] string id)
    {
        if (!Guid.TryParse(id, out var guidId))
            return BadRequest("Invalid ID format.");

        try
        {
            var response = await _sender.Send(new GetEstatePropertyByIdQuery(guidId));
            return Ok(response);
        }
        catch (Application.Common.Exceptions.NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Error fetching property {id}.", error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateEstateProperty([FromBody] CreateEstatePropertyCommand command)
    {
        try
        {
            var propertyId = await _sender.Send(command);
            var createdPropertyDto = await _sender.Send(new GetEstatePropertyByIdQuery(propertyId));
            return CreatedAtAction(nameof(GetEstateProperty), new { id = propertyId.ToString() }, createdPropertyDto);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new { message = "Validation failed.", errors = ex.Errors.Select(e => new {e.PropertyName, e.ErrorMessage}) });
        }
        catch (Exception ex)
        {
             // Log the exception ex
            return BadRequest(new { message = "Error creating property.", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEstateProperty([FromRoute] string id, [FromBody] UpdateEstatePropertyCommand command)
    {
        if (!Guid.TryParse(id, out var guidId))
            return BadRequest("Invalid ID format.");
        
        if (command.EstateProperty.Id == Guid.Empty)
            command.EstateProperty.Id = guidId;
        else if (command.EstateProperty.Id != guidId) 
            return BadRequest("Mismatched ID in route and body.");
        
        try
        {
            await _sender.Send(command);
            var updatedPropertyDto = await _sender.Send(new GetEstatePropertyByIdQuery(guidId));
            return Ok(updatedPropertyDto);
        }
        catch (Application.Common.Exceptions.NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new { message = "Validation failed.", errors = ex.Errors.Select(e => new {e.PropertyName, e.ErrorMessage}) });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Error updating property {id}.", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEstateProperty([FromRoute] string id)
    {
        if (!Guid.TryParse(id, out var guidId))
        {
            return BadRequest("Invalid ID format.");
        }
        try
        {
            await _sender.Send(new DeleteEstatePropertyCommand(guidId));
            return NoContent();
        }
        catch (Application.Common.Exceptions.NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            // Log the exception ex
            return BadRequest(new { message = $"Error deleting property {id}.", error = ex.Message });
        }
    }
}
