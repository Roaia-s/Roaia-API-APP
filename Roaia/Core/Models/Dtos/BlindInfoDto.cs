namespace Roaia.Core.Models.Dtos;

public class BlindInfoDto
{
    public string? Message { get; set; }

    [MaxLength(100, ErrorMessage = Errors.MaxLength),
        RegularExpression(RegexPatterns.GUID, ErrorMessage = Errors.InvalidGUID),
        Display(Name = "Glasses Id")]
    public string Id { get; set; }

    [MaxLength(100, ErrorMessage = Errors.MaxLength),
        RegularExpression(RegexPatterns.CharactersOnly_Eng, ErrorMessage = Errors.OnlyEnglishLetters)]
    public string? FullName { get; set; }

    [Range(10, 100)]
    public int? Age { get; set; }

    [AllowedValues("Male", "Female", ErrorMessage = Errors.AllowedGenders)]
    public string? Gender { get; set; }

    public string? ImageUrl { get; set; }

    [AllowedExtensions(FileSettings.AllowedImageExtensions, ErrorMessage = Errors.NotAllowedExtensions),
        MaxFileSize(FileSettings.MaxFileSizeInBytes, ErrorMessage = Errors.MaxSize)]
    public IFormFile? ImageUpload { get; set; }
    public List<string>? Diseases { get; set; }
}
