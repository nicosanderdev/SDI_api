using SDI_Api.Application.DTOs;
using SDI_Api.Application.DTOs.Reports;
using Sdi_Api.Application.Reports.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace SDI_Api.Web.Endpoints;

[Route("api/reports")]
[ApiController]
[Authorize]
public class ReportsController(ISender sender) : ControllerBase
{
    [HttpGet("monthly-summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMonthlySummary([FromQuery] GetMonthlySummaryQuery query)
    {
        var result = await sender.Send(query);
        return Ok(result);
    }
    
    [HttpGet("totals")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetGeneralTotals()
    {
        var result = await sender.Send(new GetGeneralTotalsQuery());
        return Ok(result);
    }
    
    [HttpGet("property-visits")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetVisitsByProperty([FromQuery] GetVisitsByPropertyQuery query)
    {
        var result = await sender.Send(query);
        return Ok(result);
    }
    
    [HttpGet("properties/{propertyId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPropertySpecificReport(string propertyId, [FromQuery] string period = "last30days")
    {
        if (!Guid.TryParse(propertyId, out var guidId))
            return BadRequest("Invalid Property ID format.");
        
        var result = await sender.Send(new GetPropertySpecificReportQuery { PropertyId = guidId, Period = period });
        return Ok(result);
    }
    
    [HttpGet("dashboard-summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDashboardSummary([FromQuery] GetDashboardSummaryQuery query)
    {
        var result = await sender.Send(query);
        return Ok(result);
    }
    
    [HttpGet("daily-visits")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDailyVisits([FromQuery] GetDailyVisitsQuery query)
    {
        var result = await sender.Send(query);
        return Ok(result);
    }
    
    [HttpGet("visits-by-source")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetVisitsBySource([FromQuery] GetVisitsBySourceQuery query)
    {
        var result = await sender.Send(query);
        return Ok(result);
    }
}
