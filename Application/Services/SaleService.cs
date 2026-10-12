using Application.DTOs;
using Application.DTOs.Sales;
using Application.Enums;
using Application.Interfaces;
using Application.Interfaces.Data;
using Domain.Entities;
using Domain.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Application.Services;

public class SaleService : ISaleService
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IOptions<TaxSettings> _taxSettings;
    private readonly IValidator<CreateSaleRequest> _validator;

    public SaleService(IAppDbContext context, ICurrentUserService currentUser, IOptions<TaxSettings> taxSettings, IValidator<CreateSaleRequest> validator)
    {
        _context = context;
        _currentUser = currentUser;
        _taxSettings = taxSettings;
        _validator = validator;
    }

    public async Task<BaseResponse<CreateSaleResponse>> CreateAsync(CreateSaleRequest request, CancellationToken ct = default)
    {
        try
        {
            var validationResult = await _validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                return new BaseResponse<CreateSaleResponse>
                {
                    Succeeded = false,
                    Message = "Validation failed",
                    Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                };
            }

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
                    return new BaseResponse<CreateSaleResponse>
                    {
                        Succeeded = false,
                        ErrorCode = ErrorCode.NotFound,
                        Message = $"Product {group.ProductId} not found.",
                        Errors = new List<string> { $"Product {group.ProductId} not found." }
                    };
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
                    UnitPrice = unitPrice,
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
                        await transaction.RollbackAsync(ct);
                        return new BaseResponse<CreateSaleResponse>
                        {
                            Succeeded = false,
                            ErrorCode = ErrorCode.Conflict,
                            Message = $"Insufficient stock for product {item.ProductId}.",
                            Errors = new List<string> { $"Insufficient stock for product {item.ProductId}." }
                        };
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

            return new BaseResponse<CreateSaleResponse>
            {
                Succeeded = true,
                Message = "Sale created successfully.",
                Data = new CreateSaleResponse
                {
                    SaleId = sale.Id,
                    Subtotal = subtotal,
                    Tax = taxTotal,
                    Total = total,
                    SaleDate = saleDate
                }
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return new BaseResponse<CreateSaleResponse>
            {
                Succeeded = false,
                ErrorCode = ErrorCode.Unexpected,
                Message = "An error occurred while creating the sale.",
                Errors = new List<string>()
            };
        }
    }

    public async Task<BaseResponse<IEnumerable<SalesReportDto>>> GetSalesReportAsync(CancellationToken ct = default)
    {
        try
        {
            var report = await _context.Sales
                .AsNoTracking()
                .Join(_context.Users.AsNoTracking(),
                    s => s.UserId,
                    u => u.Id,
                    (s, u) => new SalesReportDto
                    {
                        SaleDate = s.SaleDate,
                        SubTotal = s.SubTotal,
                        Tax = s.Tax,
                        Total = s.Total,
                        CashierFirstName = u.FirstName,
                        CashierLastName = u.LastName,
                        CashierEmail = u.Email
                    })
                .OrderByDescending(r => r.SaleDate)
                .ToListAsync(ct);

            return new BaseResponse<IEnumerable<SalesReportDto>>
            {
                Succeeded = true,
                Message = "Sales report retrieved successfully",
                Data = report
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return new BaseResponse<IEnumerable<SalesReportDto>>
            {
                Succeeded = false,
                ErrorCode = ErrorCode.Unexpected,
                Message = "An error occurred while retrieving sales report.",
                Errors = new List<string>(),
                Data = []
            };
        }
    }
}