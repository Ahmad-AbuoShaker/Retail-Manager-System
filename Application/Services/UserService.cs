using Application.DTOs;
using Application.DTOs.User;
using Application.Interface;
using Application.Interface.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IAppDbContext _appDbContext;

        public UserService(IAppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

            
        public async Task<BaseResponse<UserDto>> GetByIdAsync(Guid id, CancellationToken ct)
        {
            var user = await _appDbContext.Users.Select(u =>
                new UserDto {
                    Id = u.Id,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    CreatedDate = u.CreatedDate,
                    Role = u.Role
                }).AsNoTracking().FirstOrDefaultAsync(u => u.Id == id,ct);

            if (user == null)
            {
                return Failed("User not found");
            }

            return new BaseResponse<UserDto> { Data = user, Succeeded = true };
        }

        private static BaseResponse<UserDto> Failed(string message)
        {
            return new BaseResponse<UserDto>
            {
                Succeeded = false,
                Message = message,
                Errors = new List<string> { message }
            };
        }
    }
}
