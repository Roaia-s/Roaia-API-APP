namespace Roaia.Core.Models.Dtos;

public class GpsDto
{
    public required double Latitude { get; set; }
    public required double Longitude { get; set; }

    [MaxLength(100, ErrorMessage = Errors.MaxLength),
        RegularExpression(RegexPatterns.GUID, ErrorMessage = Errors.InvalidGUID),
        Display(Name = "Glasses Id")]
    public required string GlassesId { get; set; }
}
