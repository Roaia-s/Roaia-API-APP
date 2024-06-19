namespace Roaia.Core.Models.Dtos;

public class ModifyUserDto
{

    [MaxLength(100, ErrorMessage = Errors.MaxLength),
        RegularExpression(RegexPatterns.CharactersOnly_Eng, ErrorMessage = Errors.OnlyEnglishLetters),
        Display(Name = "First Name")]
    public string? FirstName { get; set; }

    [MaxLength(100, ErrorMessage = Errors.MaxLength),
        RegularExpression(RegexPatterns.CharactersOnly_Eng, ErrorMessage = Errors.OnlyEnglishLetters),
        Display(Name = "Last Name")]
    public string? LastName { get; set; }

    [MaxLength(20, ErrorMessage = Errors.MaxLength),
        RegularExpression(RegexPatterns.UserName, ErrorMessage = Errors.InvalidUsername),
        Display(Name = "Username")]
    public string? UserName { get; set; }

    [MaxLength(200, ErrorMessage = Errors.MaxLength), IdentifierValidator]
    public required string Email { get; set; }

    [StringLength(15, ErrorMessage = Errors.MaxMinLength),
        RegularExpression(RegexPatterns.MobileNumber, ErrorMessage = Errors.InvalidMobileNumber)]
    public string? PhoneNumber { get; set; }

    [AllowedExtensions(FileSettings.AllowedImageExtensions, ErrorMessage = Errors.NotAllowedExtensions),
            MaxFileSize(FileSettings.MaxFileSizeInBytes, ErrorMessage = Errors.MaxSize)]
    public IFormFile? ImageUrl { get; set; }

    public string? ImageName { get; set; }
}
