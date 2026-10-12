using Application.DTOs;
using Application.DTOs.Inventory;
using Application.Enums;
using Application.Interfaces;
using Application.Interfaces.Data;
using Application.Validators.Inventory;
using Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class InventoryService : IInventoryService
{
    private readonly IAppDbContext _context;
    private readonly IValidator<CreateInventoryRequest> _validator;

    public InventoryService(IAppDbContext context, IValidator<CreateInventoryRequest> validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task<BaseResponse<IEnumerable<InventoryDto>>> GetInventoryAsync(CancellationToken ct = default)
    {
        try
        {
            var inventory = await _context.Inventories
                .AsNoTracking()
                .Join(_context.Products.AsNoTracking(),
                    inv => inv.ProductId,
                    p => p.Id,
                    (inv, p) => new InventoryDto
                    {
                        Id = inv.Id,
                        ProductId = inv.ProductId,
                        ProductName = p.ProductName,
                        Quantity = inv.Quantity,
                        PurchasePrice = inv.PurchasePrice,
                        PurchaseDate = inv.PurchaseDate
                    })
                .OrderByDescending(i => i.PurchaseDate)
                .ToListAsync(ct);

            return new BaseResponse<IEnumerable<InventoryDto>>
            {
                Succeeded = true,
                Message = "Inventory retrieved successfully",
                Data = inventory
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return new BaseResponse<IEnumerable<InventoryDto>>
            {
                Succeeded = false,
                ErrorCode = ErrorCode.Unexpected,
                Message = "An error occurred while retrieving inventory.",
                Errors = new List<string>(),
                Data = []
            };
        }
    }

    public async Task<BaseResponse<InventoryDto>> CreateInventoryAsync(CreateInventoryRequest request, CancellationToken ct = default)
    {
        var validationResult = await _validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return new BaseResponse<InventoryDto>
            {
                Succeeded = false,
                Message = "Validation failed",
                Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
            };
        }

        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, ct);

        if (product == null)
        {
            return new BaseResponse<InventoryDto>
            {
                Succeeded = false,
                ErrorCode = ErrorCode.NotFound,
                Message = "Product not found",
                Errors = new List<string> { "Product not found" }
            };
        }

        var purchaseDate = request.PurchaseDate?.ToUniversalTime() ?? DateTime.UtcNow;

        var inventory = new Inventory
        {
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            PurchasePrice = request.PurchasePrice,
            PurchaseDate = purchaseDate
        };

        using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync(ct);

            var rows = await _context.Products
                .Where(p => p.Id == request.ProductId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.QuantityInStock, p => p.QuantityInStock + request.Quantity), ct);

            if (rows == 0)
            {
                await transaction.RollbackAsync(ct);
                return new BaseResponse<InventoryDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCode.NotFound,
                    Message = "Product not found",
                    Errors = new List<string> { "Product not found" }
                };
            }

            await transaction.CommitAsync(ct);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);
            Console.Error.WriteLine(ex);
            return new BaseResponse<InventoryDto>
            {
                Succeeded = false,
                ErrorCode = ErrorCode.Unexpected,
                Message = "An error occurred while creating inventory.",
                Errors = new List<string>()
            };
        }

        return new BaseResponse<InventoryDto>
        {
            Succeeded = true,
            Message = "Inventory created successfully.",
            Data = new InventoryDto
            {
                Id = inventory.Id,
                ProductId = inventory.ProductId,
                ProductName = product.ProductName,
                Quantity = inventory.Quantity,
                PurchasePrice = inventory.PurchasePrice,
                PurchaseDate = inventory.PurchaseDate
            }
        };
    }
}