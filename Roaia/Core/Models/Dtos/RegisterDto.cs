using Microsoft.AspNetCore.Mvc;

namespace Roaia.Core.Models.Dtos;

public class RegisterDto
{
    [MaxLength(100, ErrorMessage = Errors.MaxLength),
        RegularExpression(RegexPatterns.CharactersOnly_Eng, ErrorMessage = Errors.OnlyEnglishLetters),
        Display(Name = "First Name")]
    public string FirstName { get; set; } = null!;

    [MaxLength(100, ErrorMessage = Errors.MaxLength),
        RegularExpression(RegexPatterns.CharactersOnly_Eng, ErrorMessage = Errors.OnlyEnglishLetters),
        Display(Name = "Last Name")]
    public string LastName { get; set; } = null!;

    [MaxLength(20, ErrorMessage = Errors.MaxLength),
        RegularExpression(RegexPatterns.UserName, ErrorMessage = Errors.InvalidUsername),
        Display(Name = "Username")]
    public string Username { get; set; } = null!;

    [MaxLength(200, ErrorMessage = Errors.MaxLength),
        RegularExpression(RegexPatterns.Email), EmailAddress]
    public string Email { get; set; } = null!;

    [StringLength(100, ErrorMessage = Errors.MaxMinLength, MinimumLength = 8),
        RegularExpression(RegexPatterns.Password, ErrorMessage = Errors.WeakPassword),
        DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    [Display(Name = "Phone number"), MaxLength(11, ErrorMessage = Errors.MaxLength),
                RegularExpression(RegexPatterns.MobileNumber, ErrorMessage = Errors.InvalidMobileNumber)]
    public string PhoneNumber { get; set; } = null!;

    public IFormFile? ImageUrl { get; set; }

    public string? ImageName { get; set; }
}
