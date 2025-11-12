namespace Planify.Models;

public sealed class UserAccount
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public bool IsAdmin { get; set; } = false;
    public string Image { get; set; } = "missingpicture";   
}