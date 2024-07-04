namespace Roaia.Core.Models.Dtos;

public class NotificationDto
{
    [MaxLength(100, ErrorMessage = Errors.MaxLength)]
    public string Title { get; set; }

    [MaxLength(200, ErrorMessage = Errors.MaxLength)]
    public string Body { get; set; }

    [MaxLength(200, ErrorMessage = Errors.MaxLength),
        ValidUrl(ErrorMessage = Errors.InvalidImageUrl),
        Display(Name = "Image URL")]
    public string? ImageUrl { get; set; }

    [MaxLength(200, ErrorMessage = Errors.MaxLength),
        ValidUrl(ErrorMessage = Errors.InvalidAudioUrl),
        Display(Name = "Audio URL")]
    public string? AudioUrl { get; set; }

    [MaxLength(100, ErrorMessage = Errors.MaxLength),
        RegularExpression(RegexPatterns.GUID, ErrorMessage = Errors.InvalidGUID),
        Display(Name = "Glasses Id")]
    public string GlassesId { get; set; }
    [AllowedValues("Normal", "Warning", "Critical", ErrorMessage = Errors.InvalidNotificationType)]
    public string Type { get; set; }
    public string? Message { get; set; }
}
