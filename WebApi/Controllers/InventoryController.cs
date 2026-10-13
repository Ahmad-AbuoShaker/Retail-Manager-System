using Application.DTOs;
using Application.DTOs.Inventory;
using Application.Enums;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Route("api/inventory")]
[ApiController]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [Authorize(Roles = "Manager,Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
    {
        var response = await _inventoryService.GetInventoryAsync(ct);

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

    [Authorize(Roles = "WarehouseWorker,Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInventoryRequest request, CancellationToken ct = default)
    {
        var response = await _inventoryService.CreateInventoryAsync(request, ct);

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