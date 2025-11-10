using Planify.Models;

namespace Planify.Services;

public interface IUserState
{
    bool IsLoggedIn { get; set; }
    IEnumerable<UserRole> IdTokenClaims { get; set; }
}

public class UserState : IUserState
{
    public bool IsLoggedIn { get; set; } = false;
    public IEnumerable<UserRole> IdTokenClaims { get; set; } = new[]
    {
        new UserRole { Type = "user", Value = "admin" },
        new UserRole { Type = "role", Value = "tester" },
        new UserRole { Type = "environment", Value = "offline" }
    };
}