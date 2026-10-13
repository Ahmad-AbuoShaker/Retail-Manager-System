using System.Collections.Generic;

namespace Application.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }
    IReadOnlyList<string> Roles { get; }
}