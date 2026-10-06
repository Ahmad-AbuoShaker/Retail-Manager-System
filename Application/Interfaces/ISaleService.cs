using Application.DTOs.Sales;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface ISaleService
{
    Task<CreateSaleResponse> CreateAsync(CreateSaleRequest request, CancellationToken ct = default);
}