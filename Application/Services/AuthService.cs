using Application.DTOs;
using Application.DTOs.Auth;
using Application.Interface;
using Application.Interface.Data;
using Application.Interface.Security;
using FluentValidation;
using Domain.Entities;

namespace Application.Services;

public class AuthService : IAuthService
{
    private readonly IAppDbContext _appDbContext;
    private readonly IPasswordService _passwordService;
    private readonly IJwtTokenService _tokenService;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<AuthenticationRequest> _loginValidator;

    public AuthService(
        IAppDbContext appDbContext,
        IPasswordService passwordService,
        IJwtTokenService tokenService,
        IValidator<RegisterRequest> registerValidator,
        IValidator<AuthenticationRequest> loginValidator)
    {
        _appDbContext = appDbContext;
        _passwordService = passwordService;
        _tokenService = tokenService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    public async Task<BaseResponse<AuthenticationResponse>> RegisterAsync(RegisterRequest request)
    {
        var validationResult = await _registerValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return new BaseResponse<AuthenticationResponse>
            {
                Succeeded = false,
                Message = "Validation failed",
                Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
            };
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var existingUser = _appDbContext.Users.FirstOrDefault(u => u.Email == normalizedEmail);
        if (existingUser != null)
        {
            return new BaseResponse<AuthenticationResponse>
            {
                Succeeded = false,
                Message = "Email already registered",
                Errors = new List<string> { "Email already registered" }
            };
        }

        var passwordHash = _passwordService.HashPassword(request.Password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            PasswordHash = passwordHash,
            Role = "Customer",
            CreatedDate = DateTime.UtcNow
        };

        _appDbContext.Users.Add(user);
        await _appDbContext.SaveChangesAsync();

        var accessToken = _tokenService.GenerateAccessToken(user);

        return new BaseResponse<AuthenticationResponse>
        {
            Succeeded = true,
            Message = "Registration successful",
            Data = new AuthenticationResponse { AccessToken = accessToken }
        };
    }

    public async Task<BaseResponse<AuthenticationResponse>> LoginAsync(AuthenticationRequest request)
    {
        var validationResult = await _loginValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return new BaseResponse<AuthenticationResponse>
            {
                Succeeded = false,
                Message = "Validation failed",
                Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
            };
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = _appDbContext.Users.FirstOrDefault(u => u.Email == normalizedEmail);
        if (user == null || !_passwordService.VerifyPassword(request.Password, user.PasswordHash))
        {
            return new BaseResponse<AuthenticationResponse>
            {
                Succeeded = false,
                Message = "Invalid credentials",
                Errors = new List<string> { "Invalid credentials" }
            };
        }

        var accessToken = _tokenService.GenerateAccessToken(user);

        return new BaseResponse<AuthenticationResponse>
        {
            Succeeded = true,
            Message = "Login successful",
            Data = new AuthenticationResponse { AccessToken = accessToken }
        };
    }
}