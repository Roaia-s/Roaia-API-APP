namespace Roaia.Core.Models.Dtos;

public class TokenRequestDto
{
    [MaxLength(128)]
    public string Email { get; set; } = null!;

    [MaxLength(100)]
    public string Password { get; set; } = null!;
}
