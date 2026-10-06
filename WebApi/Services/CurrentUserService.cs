using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace WebApi.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || string.IsNullOrWhiteSpace(userIdClaim.Value))
            {
                throw new InvalidOperationException("User ID claim is missing.");
            }

            if (!Guid.TryParse(userIdClaim.Value, out var userId))
            {
                throw new InvalidOperationException("User ID claim is not a valid GUID.");
            }

            return userId;
        }
    }
}