namespace Planify.Models;

public sealed class UserAccount
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public bool IsAdmin { get; set; } = false;

    // Flexible metadata: can store any object (image bytes, settings, etc.)
    public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
}