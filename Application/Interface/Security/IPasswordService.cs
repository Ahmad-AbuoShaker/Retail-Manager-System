namespace Application.Interface.Security
{
    public interface IPasswordService
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
        bool VerifyPassword(string password, object passwordHash);
    }
}
