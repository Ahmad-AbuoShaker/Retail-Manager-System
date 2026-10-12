using Application.DTOs;
using Application.DTOs.Sales;
using Application.Enums;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Authorize(Roles = "Cashier")]
[Route("api/sales")]
[ApiController]
public class SalesController : ControllerBase
{
    private readonly ISaleService _saleService;

    public SalesController(ISaleService saleService)
    {
        _saleService = saleService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSaleRequest request, CancellationToken ct = default)
    {
        var response = await _saleService.CreateAsync(request, ct);

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