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
    }
}