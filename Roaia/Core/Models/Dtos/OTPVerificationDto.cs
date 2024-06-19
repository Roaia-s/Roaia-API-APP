namespace Roaia.Core.Models.Dtos;

public class OTPVerificationDto
{
    [MaxLength(200, ErrorMessage = Errors.MaxLength), IdentifierValidator]
    public required string Email { get; set; }
    public required string OtpCode { get; set; }
}
