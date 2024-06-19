namespace Roaia.Core.Models.Dtos;

public class ContactDto
{
    public int? Id { get; set; }
    [MaxLength(100, ErrorMessage = Errors.MaxLength),
        RegularExpression(RegexPatterns.CharactersOnly_Eng, ErrorMessage = Errors.OnlyEnglishLetters)]
    public string Name { get; set; }

    [Range(10, 100)]
    public int? Age { get; set; }

    [MaxLength(20, ErrorMessage = Errors.MaxLength),
        RegularExpression(RegexPatterns.CharactersOnly_Eng, ErrorMessage = Errors.OnlyEnglishLetters)]
    public string? Relation { get; set; }

    [AllowedExtensions(FileSettings.AllowedImageExtensions, ErrorMessage = Errors.NotAllowedExtensions),
        MaxFileSize(FileSettings.MaxFileSizeInBytes, ErrorMessage = Errors.MaxSize)]
    public IFormFile? ImageUpload { get; set; }

    [StringLength(15, ErrorMessage = Errors.MaxMinLength),
        RegularExpression(RegexPatterns.MobileNumber, ErrorMessage = Errors.InvalidMobileNumber)]
    public string? PhoneNumber { get; set; }

    public string? ImageUrl { get; set; }

    [MaxLength(100, ErrorMessage = Errors.MaxLength),
        RegularExpression(RegexPatterns.GUID, ErrorMessage = Errors.InvalidGUID),
        Display(Name = "Glasses Id")]
    public string? BlindId { get; set; }
    public string? Message { get; set; }
}