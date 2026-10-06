namespace Application.Interfaces;

// Provides the authenticated user's ID from the JWT token.
public interface ICurrentUserService
{
    Guid UserId { get; }
}