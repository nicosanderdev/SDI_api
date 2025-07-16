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
public class ReportsController : ControllerBase
{
    private readonly ISender _sender;

    public ReportsController(ISender sender)
    {
        _sender = sender;
    }
    
    [HttpGet("monthly-summary")]
    public async Task<ActionResult<MonthlySummaryDataDto>> GetMonthlySummary([FromQuery] GetMonthlySummaryQuery query)
    {
        var result = await _sender.Send(query);
        return Ok(result);
    }
    
    [HttpGet("totals")]
    public async Task<ActionResult<GeneralTotalsDataDto>> GetGeneralTotals()
    {
        var result = await _sender.Send(new GetGeneralTotalsQuery());
        return Ok(result);
    }
    
    [HttpGet("property-visits")]
    public async Task<ActionResult<VisitsByPropertyDataDto>> GetVisitsByProperty([FromQuery] GetVisitsByPropertyQuery query)
    {
        var result = await _sender.Send(query);
        return Ok(result);
    }
    
    [HttpGet("properties/{propertyId}")]
    public async Task<ActionResult<PropertySpecificReportDataDto>> GetPropertySpecificReport(string propertyId, [FromQuery] string period = "last30days")
    {
        if (!Guid.TryParse(propertyId, out var guidId))
            return BadRequest("Invalid Property ID format.");
        
        var result = await _sender.Send(new GetPropertySpecificReportQuery { PropertyId = guidId, Period = period });
        return Ok(result);
    }
    
    [HttpGet("dashboard-summary")]
    public async Task<ActionResult<DashboardSummaryDataDto>> GetDashboardSummary([FromQuery] GetDashboardSummaryQuery query)
    {
        var result = await _sender.Send(query);
        return Ok(result);
    }
    
    [HttpGet("daily-visits")]
    public async Task<ActionResult<List<DailyVisitDto>>> GetDailyVisits([FromQuery] GetDailyVisitsQuery query)
    {
        var result = await _sender.Send(query);
        return Ok(result);
    }
    
    [HttpGet("visits-by-source")]
    public async Task<ActionResult<List<VisitSourceDto>>> GetVisitsBySource([FromQuery] GetVisitsBySourceQuery query)
    {
        var result = await _sender.Send(query);
        return Ok(result);
    }
}
