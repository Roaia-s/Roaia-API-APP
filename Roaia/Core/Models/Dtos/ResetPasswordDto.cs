namespace Roaia.Core.Models.Dtos;

public class ResetPasswordDto
{
	public required string Email { get; set; }
	public required string NewPassword { get; set; }
	public required string ConfirmPassword { get; set; }
}
