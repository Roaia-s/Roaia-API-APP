namespace Roaia.Core.Models.Dtos;

public class ChangePasswordDto
{
    [MaxLength(200, ErrorMessage = Errors.MaxLength), IdentifierValidator]
    public required string Email { get; set; }
    public required string CurrentPassword { get; set; }

    [StringLength(100, ErrorMessage = Errors.MaxMinLength, MinimumLength = 8),
        RegularExpression(RegexPatterns.Password, ErrorMessage = Errors.WeakPassword),
        DataType(DataType.Password),
        Display(Name = "New password")]
    public required string NewPassword { get; set; }

    [DataType(DataType.Password),
        Compare("NewPassword", ErrorMessage = Errors.ConfirmPasswordNotMatch),
        Display(Name = "Confirm password")]
    public required string ConfirmPassword { get; set; }
}
