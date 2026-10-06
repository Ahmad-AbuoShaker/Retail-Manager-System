using Application.DTOs.Sales;
using Application.Interfaces;
using Application.Interface.Data;
using Domain.Entities;
using Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Application.Services;

public class SaleService : ISaleService
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IOptions<TaxSettings> _taxSettings;

    public SaleService(IAppDbContext context, ICurrentUserService currentUser,IOptions<TaxSettings> taxSettings)
    {
        _context = context;
        _currentUser = currentUser;
        _taxSettings = taxSettings;
    }

    public async Task<CreateSaleResponse> CreateAsync(CreateSaleRequest request, CancellationToken ct = default)
    {
        var cashierId = _currentUser.UserId;

        var itemGroups = request.Items
            .GroupBy(i => i.ProductId)
            .Select(g => new { ProductId = g.Key, Quantity = g.Sum(i => i.Quantity) })
            .ToList();

        var productIds = itemGroups.Select(g => g.ProductId).ToList();

        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .Select(p => new { p.Id, p.RetailPrice, p.IsTaxable, p.QuantityInStock })
            .ToDictionaryAsync(p => p.Id, ct);

        foreach (var group in itemGroups)
        {
            if (!products.TryGetValue(group.ProductId, out var product))
            {
                throw new KeyNotFoundException($"Product {group.ProductId} not found.");
            }

            if (product.QuantityInStock < group.Quantity)
            {
                throw new InvalidOperationException($"Insufficient stock for product {group.ProductId}. Available: {product.QuantityInStock}, requested: {group.Quantity}.");
            }
        }

        var saleDate = DateTime.UtcNow;
        var details = new List<SaleDetail>();
        decimal subtotal = 0m;
        decimal taxTotal = 0m;

        foreach (var group in itemGroups)
        {
            var product = products[group.ProductId];
            var unitPrice = product.RetailPrice;
            var lineSubtotal = unitPrice * group.Quantity;
            var lineTax = product.IsTaxable
                ? TaxCalculator.CalculateTax(lineSubtotal, _taxSettings.Value.Rate)
                : 0m;

            subtotal += lineSubtotal;
            taxTotal += lineTax;

            details.Add(new SaleDetail
            {
                ProductId = group.ProductId,
                Quantity = group.Quantity,
                PurchasePrice = unitPrice,
                Tax = lineTax
            });
        }

        var total = subtotal + taxTotal;

        var sale = new Sale
        {
            UserId = cashierId,
            SaleDate = saleDate,
            SubTotal = subtotal,
            Tax = taxTotal,
            Total = total,
            SaleDetails = details
        };

        using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            foreach (var item in itemGroups)
            {
                var rows = await _context.Products
                    .Where(p => p.Id == item.ProductId && p.QuantityInStock >= item.Quantity)
                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.QuantityInStock, p => p.QuantityInStock - item.Quantity), ct);

                if (rows == 0)
                {
                    throw new InvalidOperationException($"Concurrent stock change for product {item.ProductId}. Please retry.");
                }
            }

            _context.Sales.Add(sale);
            await _context.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }

        return new CreateSaleResponse
        {
            SaleId = sale.Id,
            Subtotal = subtotal,
            Tax = taxTotal,
            Total = total,
            SaleDate = saleDate
        };
    }
}