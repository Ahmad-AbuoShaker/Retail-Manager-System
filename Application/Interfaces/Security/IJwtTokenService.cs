using Domain.Entities;
using System.Collections.Generic;

namespace Application.Interfaces.Security
{
    public interface IJwtTokenService
    {
        string GenerateAccessToken(User user, List<string> roles);
        string GenerateRefreshToken();
        string Hash(string token);
    }
}