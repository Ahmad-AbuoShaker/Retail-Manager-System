using Application.DTOs;
using Application.DTOs.Auth;
using Application.Interface;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var response = await _authService.RegisterAsync(request);
        
        if (!response.Succeeded)
        {
            if (response.Message == "Email already registered")
            {
                return Conflict(response);
            }
            return BadRequest(response);
        }

        return Created("", response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthenticationRequest request)
    {
        var response = await _authService.LoginAsync(request);
        
        if (!response.Succeeded)
        {
            return Unauthorized(response);
        }

        return Ok(response);
    }
}