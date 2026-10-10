using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Security
{
    public interface IJwtTokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        string Hash(string token);
    }
}
