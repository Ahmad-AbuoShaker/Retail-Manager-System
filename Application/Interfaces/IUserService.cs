using Application.DTOs;
using Application.DTOs.User;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserService
    {
        Task<BaseResponse<UserDto>> GetByIdAsync(Guid id, CancellationToken ct);
        Task<BaseResponse<List<UserListDto>>> GetAllUsersAsync(CancellationToken ct);
        Task<BaseResponse<Dictionary<string, string>>> GetAllRolesAsync(CancellationToken ct);
        Task<BaseResponse<bool>> AddRoleAsync(UserRolePairDto request, CancellationToken ct);
        Task<BaseResponse<bool>> RemoveRoleAsync(UserRolePairDto request, CancellationToken ct);
    }
}
