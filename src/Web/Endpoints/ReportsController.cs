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

    // GET /api/reports/monthly-summary?year=2023&month=10
    [HttpGet("monthly-summary")]
    public async Task<ActionResult<MonthlySummaryDataDto>> GetMonthlySummary([FromQuery] GetMonthlySummaryQuery query)
    {
        try
        {
            var result = await _sender.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            // Log ex
            return BadRequest(new { message = "Error fetching monthly summary.", error = ex.Message });
        }
    }

    // GET /api/reports/totals
    [HttpGet("totals")]
    public async Task<ActionResult<GeneralTotalsDataDto>> GetGeneralTotals()
    {
        try
        {
            var result = await _sender.Send(new GetGeneralTotalsQuery());
            return Ok(result);
        }
        catch (Exception ex)
        {
            // Log ex
            return BadRequest(new { message = "Error fetching general totals.", error = ex.Message });
        }
    }

    // GET /api/reports/property-visits?period=last7days&page=1&limit=10
    [HttpGet("property-visits")]
    public async Task<ActionResult<VisitsByPropertyDataDto>> GetVisitsByProperty([FromQuery] GetVisitsByPropertyQuery query)
    {
        try
        {
            var result = await _sender.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            // Log ex
            return BadRequest(new { message = "Error fetching visits by property.", error = ex.Message });
        }
    }

    // GET /api/reports/properties/{propertyId}?period=last30days
    [HttpGet("properties/{propertyId}")]
    public async Task<ActionResult<PropertySpecificReportDataDto>> GetPropertySpecificReport(string propertyId, [FromQuery] string period = "last30days")
    {
        if (!Guid.TryParse(propertyId, out var guidId))
        {
            return BadRequest("Invalid Property ID format.");
        }
        try
        {
            var result = await _sender.Send(new GetPropertySpecificReportQuery { PropertyId = guidId, Period = period });
            return Ok(result);
        }
        catch (Application.Common.Exceptions.NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            // Log ex
            return BadRequest(new { message = $"Error fetching report for property {propertyId}.", error = ex.Message });
        }
    }

    // GET /api/reports/dashboard-summary?period=last30days
    [HttpGet("dashboard-summary")]
    public async Task<ActionResult<DashboardSummaryDataDto>> GetDashboardSummary([FromQuery] GetDashboardSummaryQuery query)
    {
         try
        {
            var result = await _sender.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            // Log ex
            return BadRequest(new { message = "Error fetching dashboard summary.", error = ex.Message });
        }
    }

    // GET /api/reports/daily-visits?period=last7days&dateFormat=dd/MM
    [HttpGet("daily-visits")]
    public async Task<ActionResult<List<DailyVisitDto>>> GetDailyVisits([FromQuery] GetDailyVisitsQuery query)
    {
        try
        {
            var result = await _sender.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            // Log ex
            return BadRequest(new { message = "Error fetching daily visits.", error = ex.Message });
        }
    }

    // GET /api/reports/visits-by-source?period=last30days
    [HttpGet("visits-by-source")]
    public async Task<ActionResult<List<VisitSourceDto>>> GetVisitsBySource([FromQuery] GetVisitsBySourceQuery query)
    {
        try
        {
            var result = await _sender.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            // Log ex
            return BadRequest(new { message = "Error fetching visits by source.", error = ex.Message });
        }
    }
}
