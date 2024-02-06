namespace Roaia.Core.Models.Dtos;

public class ChangePasswordDto
{
	public required string Email { get; set; }
	public required string OldPassword { get; set; }
	public required string NewPassword { get; set; }
}
