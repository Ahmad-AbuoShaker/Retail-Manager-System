using Application.DTOs;
using Application.DTOs.User;
using Application.Enums;
using Application.Interfaces;
using Application.Interfaces.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
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
                return Failed<UserDto>("User not found", ErrorCode.NotFound);
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

        public async Task<BaseResponse<List<UserListDto>>> GetAllUsersAsync(CancellationToken ct)
        {
            var users = await _appDbContext.Users
                .AsNoTracking()
                .Select(u => new UserListDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
                })
                .ToListAsync(ct);

            return new BaseResponse<List<UserListDto>> { Data = users, Succeeded = true };
        }

        public async Task<BaseResponse<Dictionary<string, string>>> GetAllRolesAsync(CancellationToken ct)
        {
            var roles = await _appDbContext.Roles
                .AsNoTracking()
                .ToDictionaryAsync(r => r.Id.ToString(), r => r.Name, ct);

            return new BaseResponse<Dictionary<string, string>> { Data = roles, Succeeded = true };
        }

        public async Task<BaseResponse<bool>> AddRoleAsync(UserRolePairDto request, CancellationToken ct)
        {
            var roleName = request.RoleName.Trim();

            var user = await _appDbContext.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, ct);

            if (user == null)
            {
                return Failed<bool>("User not found", ErrorCode.NotFound);
            }

            var role = await _appDbContext.Roles.FirstOrDefaultAsync(r => r.Name == roleName, ct);
            if (role == null)
            {
                return Failed<bool>("Role not found", ErrorCode.NotFound);
            }

            var alreadyHasRole = user.UserRoles.Any(ur => ur.RoleId == role.Id);
            if (alreadyHasRole)
            {
                return Failed<bool>("User already has this role", ErrorCode.Conflict);
            }

            _appDbContext.UserRoles.Add(new Domain.Entities.UserRole { UserId = user.Id, RoleId = role.Id });
            await _appDbContext.SaveChangesAsync(ct);

            return new BaseResponse<bool> { Data = true, Succeeded = true };
        }

        public async Task<BaseResponse<bool>> RemoveRoleAsync(UserRolePairDto request, CancellationToken ct)
        {
            var roleName = request.RoleName.Trim();

            var user = await _appDbContext.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, ct);

            if (user == null)
            {
                return Failed<bool>("User not found", ErrorCode.NotFound);
            }

            var role = await _appDbContext.Roles.FirstOrDefaultAsync(r => r.Name == roleName, ct);
            if (role == null)
            {
                return Failed<bool>("Role not found", ErrorCode.NotFound);
            }

            var userRole = user.UserRoles.FirstOrDefault(ur => ur.RoleId == role.Id);
            if (userRole == null)
            {
                return Failed<bool>("User does not have this role", ErrorCode.NotFound);
            }

            _appDbContext.UserRoles.Remove(userRole);
            await _appDbContext.SaveChangesAsync(ct);

            return new BaseResponse<bool> { Data = true, Succeeded = true };
        }

        private static BaseResponse<T> Failed<T>(string message, ErrorCode errorCode)
        {
            return new BaseResponse<T>
            {
                Succeeded = false,
                Message = message,
                Errors = new List<string> { message },
                ErrorCode = errorCode
            };
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