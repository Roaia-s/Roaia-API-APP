namespace Roaia.Core.Models.Dtos;

public class BlindInfoDto
{
	public string? Message { get; set; }
	public string Id { get; set; }
	public string? FullName { get; set; }
	public int? Age { get; set; }
	public string? Gender { get; set; }
	public string? ImageUrl { get; set; }
	public IFormFile? ImageUpload { get; set; }
	public List<string>? Diseases { get; set; }
}
