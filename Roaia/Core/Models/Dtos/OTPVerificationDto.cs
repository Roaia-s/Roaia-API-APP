namespace Roaia.Core.Models.Dtos;

public class OTPVerificationDto
{
	public required string Email { get; set; }
	public required string OtpCode { get; set; }
}
