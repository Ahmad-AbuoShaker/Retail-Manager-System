using Application.DTOs;
using Application.DTOs.User;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserService
    {
        Task<BaseResponse<UserDto>> GetByIdAsync(Guid id, CancellationToken ct);
    }
}
