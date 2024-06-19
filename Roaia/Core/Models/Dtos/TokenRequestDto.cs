namespace Roaia.Core.Models.Dtos;

public class TokenRequestDto
{
    [MaxLength(200, ErrorMessage = Errors.MaxLength), IdentifierValidator]
    public required string Email { get; set; }

    [MaxLength(100)]
    public required string Password { get; set; }

    public string? DeviceToken { get; set; }
}
