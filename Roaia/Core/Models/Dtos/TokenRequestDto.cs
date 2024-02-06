namespace Roaia.Core.Models.Dtos;

public class TokenRequestDto
{
	[MaxLength(128)]
	public required string Email { get; set; }

	[MaxLength(100)]
	public required string Password { get; set; }
}
