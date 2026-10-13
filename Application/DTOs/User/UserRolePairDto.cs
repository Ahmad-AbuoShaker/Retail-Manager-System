namespace Application.DTOs.User
{
    public class UserRolePairDto
    {
        public Guid UserId { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }
}