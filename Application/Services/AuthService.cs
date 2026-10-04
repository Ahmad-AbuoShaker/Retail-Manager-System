using Application.DTOs;
using Application.DTOs.Auth;
using Application.Interface;
using Application.Interface.Data;
using Application.Interface.Security;
using FluentValidation;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

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

    public async Task<BaseResponse<AuthenticationResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var validationResult = await _registerValidator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return Failed(validationResult.Errors.Select(e => e.ErrorMessage).ToList(), "Validation failed");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var firstName = request.FirstName.Trim();
        var lastName = request.LastName.Trim();

        var emailExists = await _appDbContext.Users.AnyAsync(u => u.Email == normalizedEmail, ct);
        if (emailExists)
        {
            return Failed("Email already registered");
        }

        var passwordHash = _passwordService.HashPassword(request.Password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            PasswordHash = passwordHash,
            Role = "Cashier",
            FirstName = firstName,
            LastName = lastName,
            CreatedDate = DateTime.UtcNow
        };

        try
        {
            _appDbContext.Users.Add(user);
            await _appDbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return Failed("Email already registered");
        }

        var accessToken = _tokenService.GenerateAccessToken(user);

        return new BaseResponse<AuthenticationResponse>
        {
            Succeeded = true,
            Message = "Registration successful",
            Data = new AuthenticationResponse { AccessToken = accessToken }
        };
    }

    public async Task<BaseResponse<AuthenticationResponse>> LoginAsync(AuthenticationRequest request, CancellationToken ct)
    {
        var validationResult = await _loginValidator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return Failed(validationResult.Errors.Select(e => e.ErrorMessage).ToList(), "Validation failed");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, ct);

        if (user == null || !_passwordService.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Failed("Invalid credentials");
        }

        var accessToken = _tokenService.GenerateAccessToken(user);

        return new BaseResponse<AuthenticationResponse>
        {
            Succeeded = true,
            Message = "Login successful",
            Data = new AuthenticationResponse { AccessToken = accessToken }
        };
    }

    private static BaseResponse<AuthenticationResponse> Failed(List<string> errors, string message)
    {
        return new BaseResponse<AuthenticationResponse>
        {
            Succeeded = false,
            Message = message,
            Errors = errors
        };
    }

    private static BaseResponse<AuthenticationResponse> Failed(string message)
    {
        return new BaseResponse<AuthenticationResponse>
        {
            Succeeded = false,
            Message = message,
            Errors = new List<string> { message }
        };
    }

}