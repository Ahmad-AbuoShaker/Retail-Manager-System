using Application.DTOs;
using Application.DTOs.Inventory;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface IInventoryService
{
    Task<BaseResponse<IEnumerable<InventoryDto>>> GetInventoryAsync(CancellationToken ct = default);
    Task<BaseResponse<InventoryDto>> CreateInventoryAsync(CreateInventoryRequest request, CancellationToken ct = default);
}