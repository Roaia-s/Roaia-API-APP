using Microsoft.AspNetCore.Mvc.Rendering;

namespace Roaia.Core.Models.Dtos;

public class UserFormDto
{
    public string? Id { get; set; }

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
    public string UserName { get; set; } = null!;

    [MaxLength(200, ErrorMessage = Errors.MaxLength),
        RegularExpression(RegexPatterns.Email), EmailAddress]
    public string Email { get; set; } = null!;

    [StringLength(100, ErrorMessage = Errors.MaxMinLength, MinimumLength = 8),
        RegularExpression(RegexPatterns.Password, ErrorMessage = Errors.WeakPassword),
        DataType(DataType.Password)]
    public string? Password { get; set; } = null!;

    [DataType(DataType.Password),
        Compare("Password", ErrorMessage = Errors.ConfirmPasswordNotMatch),
        Display(Name = "Confirm password")]
    public string? ConfirmPassword { get; set; } = null!;

    [Display(Name = "Roles")]
    public IList<string> SelectedRoles { get; set; } = new List<string>();

    public IEnumerable<SelectListItem>? Roles { get; set; }
}
