using Application.DTOs;
using Application.DTOs.User;
using Application.Interfaces;
using Application.Interfaces.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
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
            var user = await _appDbContext.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id, ct);

            if (user == null)
            {
                return Failed("User not found");
            }

            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();

            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                CreatedDate = user.CreatedDate,
                Roles = roles
            };

            return new BaseResponse<UserDto> { Data = userDto, Succeeded = true };
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