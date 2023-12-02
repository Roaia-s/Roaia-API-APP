namespace Roaia.Core.Models;

public class TokenRequest
{
    [MaxLength(128)]
    public string Email { get; set; } = null!;

    [MaxLength(256)]
    public string Password { get; set; } = null!;
}
