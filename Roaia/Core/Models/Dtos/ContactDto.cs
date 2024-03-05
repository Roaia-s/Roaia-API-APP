namespace Roaia.Core.Models.Dtos;

public class ContactDto
{
	public string Name { get; set; }
	public int? Age { get; set; }
	public string? Relation { get; set; }
	public IFormFile? ImageUpload { get; set; }
	public string? PhoneNumber { get; set; }
	public string? ImageUrl { get; set; }
	public string? BlindId { get; set; }
	public string? Message { get; set; }
}