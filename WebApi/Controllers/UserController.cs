using Application.DTOs;
using Application.DTOs.User;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/user")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("me")]
        [ProducesResponseType(typeof(BaseResponse<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<UserDto>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BaseResponse<UserDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCurrentUser(CancellationToken ct)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var result = await _userService.GetByIdAsync(userId,ct);
            return result.Succeeded ? Ok(result) : NotFound(result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(BaseResponse<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<UserDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<UserDto>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BaseResponse<UserDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var result = await _userService.GetByIdAsync(id, ct);
            return result.Succeeded ? Ok(result) : NotFound(result);
        }

        [HttpGet("admin/getallusers")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(BaseResponse<List<UserListDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<List<UserListDto>>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BaseResponse<List<UserListDto>>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllUsers(CancellationToken ct)
        {
            var result = await _userService.GetAllUsersAsync(ct);
            return result.Succeeded ? Ok(result) : StatusCode(403, result);
        }

        [HttpGet("admin/roles")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(BaseResponse<Dictionary<string, string>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<Dictionary<string, string>>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BaseResponse<Dictionary<string, string>>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllRoles(CancellationToken ct)
        {
            var result = await _userService.GetAllRolesAsync(ct);
            return result.Succeeded ? Ok(result) : StatusCode(403, result);
        }

        [HttpPost("admin/addrole")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AddRole([FromBody] UserRolePairDto request, CancellationToken ct)
        {
            var result = await _userService.AddRoleAsync(request, ct);
            if (!result.Succeeded)
            {
                return result.ErrorCode switch
                {
                    Application.Enums.ErrorCode.NotFound => NotFound(result),
                    Application.Enums.ErrorCode.Conflict => Conflict(result),
                    _ => BadRequest(result)
                };
            }
            return Ok(result);
        }

        [HttpPost("admin/removerole")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveRole([FromBody] UserRolePairDto request, CancellationToken ct)
        {
            var result = await _userService.RemoveRoleAsync(request, ct);
            if (!result.Succeeded)
            {
                return result.ErrorCode switch
                {
                    Application.Enums.ErrorCode.NotFound => NotFound(result),
                    _ => BadRequest(result)
                };
            }
            return Ok(result);
        }
    }
}