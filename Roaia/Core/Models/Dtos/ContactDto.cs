namespace Roaia.Core.Models.Dtos;

public class ContactDto
{
	public string Name { get; set; } = string.Empty;
	public int? Age { get; set; }
	public string? Relation { get; set; } = string.Empty;
	public IFormFile? ImageUpload { get; set; }
	public string? ImageUrl { get; set; } = string.Empty;
	public string? BlindId { get; set; } = string.Empty;
	public string? Message { get; set; } = string.Empty;
}