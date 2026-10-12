using Application.DTOs;
using Application.DTOs.Sales;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces;

// Handles sale creation: validates products, calculates totals, deducts stock atomically.
public interface ISaleService
{
    Task<BaseResponse<CreateSaleResponse>> CreateAsync(CreateSaleRequest request, CancellationToken ct = default);
    Task<BaseResponse<IEnumerable<SalesReportDto>>> GetSalesReportAsync(CancellationToken ct = default);
}