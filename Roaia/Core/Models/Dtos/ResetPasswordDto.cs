namespace Roaia.Core.Models.Dtos;

public class ResetPasswordDto
{
    [MaxLength(200, ErrorMessage = Errors.MaxLength), IdentifierValidator]
    public required string Email { get; set; }
    public string? Token { get; set; }

    [StringLength(100, ErrorMessage = Errors.MaxMinLength, MinimumLength = 8)]
    [RegularExpression(RegexPatterns.Password, ErrorMessage = Errors.WeakPassword)]
    [DataType(DataType.Password)]
    [Display(Name = "New password")]
    public required string NewPassword { get; set; } = null!;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("NewPassword", ErrorMessage = Errors.ConfirmPasswordNotMatch)]
    public required string ConfirmPassword { get; set; } = null!;
}
