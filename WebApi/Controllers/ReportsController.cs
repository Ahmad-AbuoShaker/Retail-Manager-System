using Application.DTOs;
using Application.DTOs.Sales;
using Application.Enums;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Authorize]
[Route("api/reports")]
[ApiController]
public class ReportsController : ControllerBase
{
    private readonly ISaleService _saleService;

    public ReportsController(ISaleService saleService)
    {
        _saleService = saleService;
    }

    // TODO: restrict to admin role
    [HttpGet("sales")]
    public async Task<IActionResult> GetSalesReport(CancellationToken ct = default)
    {
        var response = await _saleService.GetSalesReportAsync(ct);

        if (!response.Succeeded)
        {
            return response.ErrorCode switch
            {
                ErrorCode.NotFound => NotFound(response),
                ErrorCode.Conflict => Conflict(response),
                ErrorCode.Unexpected => StatusCode(StatusCodes.Status500InternalServerError, response),
                _ => BadRequest(response)
            };
        }

        return Ok(response);
    }
}