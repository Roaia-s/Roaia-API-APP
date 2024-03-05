namespace Roaia.Core.Models.Dtos;

public class RegisterDto
{
	public required string FirstName { get; set; }

	public required string LastName { get; set; }

	public required string Username { get; set; }

	public required string BlindId { get; set; }

	public required string Email { get; set; }

	public required string Password { get; set; }

	public string? PhoneNumber { get; set; }

	public IFormFile? ImageUrl { get; set; }

	public bool IsAgree { get; set; }

	public string? ImageName { get; set; }
}
