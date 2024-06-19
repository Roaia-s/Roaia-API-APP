namespace Roaia.Core.Models.Dtos;

public class RegisterDto
{
    [MaxLength(100, ErrorMessage = Errors.MaxLength),
        RegularExpression(RegexPatterns.CharactersOnly_Eng, ErrorMessage = Errors.OnlyEnglishLetters),
        Display(Name = "First Name")]
    public required string FirstName { get; set; }

    [MaxLength(100, ErrorMessage = Errors.MaxLength),
        RegularExpression(RegexPatterns.CharactersOnly_Eng, ErrorMessage = Errors.OnlyEnglishLetters),
        Display(Name = "Last Name")]
    public required string LastName { get; set; }

    [MaxLength(20, ErrorMessage = Errors.MaxLength),
        RegularExpression(RegexPatterns.UserName, ErrorMessage = Errors.InvalidUsername),
        Display(Name = "Username")]
    public required string Username { get; set; }

    [MaxLength(100, ErrorMessage = Errors.MaxLength),
        RegularExpression(RegexPatterns.GUID, ErrorMessage = Errors.InvalidGUID),
        Display(Name = "Glasses Id")]
    public required string BlindId { get; set; }

    [MaxLength(200, ErrorMessage = Errors.MaxLength),
        RegularExpression(RegexPatterns.Email, ErrorMessage = Errors.InvalidEmail), EmailAddress]
    public required string Email { get; set; }

    [StringLength(100, ErrorMessage = Errors.MaxMinLength, MinimumLength = 8),
        RegularExpression(RegexPatterns.Password, ErrorMessage = Errors.WeakPassword),
        DataType(DataType.Password)]
    public required string Password { get; set; }

    [StringLength(15, ErrorMessage = Errors.MaxMinLength),
        RegularExpression(RegexPatterns.MobileNumber, ErrorMessage = Errors.InvalidMobileNumber)]
    public string? PhoneNumber { get; set; }

    [AllowedExtensions(FileSettings.AllowedImageExtensions, ErrorMessage = Errors.NotAllowedExtensions),
        MaxFileSize(FileSettings.MaxFileSizeInBytes, ErrorMessage = Errors.MaxSize)]
    public IFormFile? ImageUrl { get; set; }
    // alow isAgree to be always true
    [Range(typeof(bool), "true", "true", ErrorMessage = "You must agree to the terms and conditions.")]
    public required bool IsAgree { get; set; }

    public string? ImageName { get; set; }
}
