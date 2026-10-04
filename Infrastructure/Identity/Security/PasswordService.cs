using Application.Interface.Security;
using BCrypt.Net;

namespace Infrastructure.Identity.Security;

public class PasswordService : IPasswordService
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }

    public bool VerifyPassword(string password, object passwordHash)
    {
        return VerifyPassword(password, passwordHash?.ToString() ?? string.Empty);
    }
}