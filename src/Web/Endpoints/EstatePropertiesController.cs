using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.EstateProperties.Queries;
using SDI_Api.Application.EstateProperties.Commands;
using SDI_Api.Application.EstateProperties.Commands.Create;
using SDI_Api.Domain.Entities;
using SDI_Api.Web.Dtos;

namespace SDI_Api.Web.Endpoints;

[Route("api/estate-properties/")]
[ApiController]
public class EstatePropertiesController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IMapper _mapper;

    public EstatePropertiesController(ISender sender, IMapper mapper)
    {
        _mapper = mapper;
        _sender = sender;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetEstateProperties()
    {
        IReadOnlyCollection<EstateProperty> response; 
        try
        {
            response = await _sender.Send(new GetEstatePropertiesQuery());
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

        return Ok(_mapper.Map<List<EstatePropertyDto>>(response));
    }

    [HttpPost]
    public async Task<IActionResult> CreateEstateProperty([FromBody] EstatePropertyDto estatePropertyDto)
    {
        try
        {
            await _sender.Send(new CreateEstatePropertyCommand(_mapper.Map<CreateEstatePropertyRequest>(estatePropertyDto)));
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

        return Ok();
    }
}
