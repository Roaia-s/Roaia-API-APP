namespace Roaia.Core.Models.Dtos;

public class UserInfoDto
{
	public string Id { get; set; }
	public string FirstName { get; set; } = null!;
	public string LastName { get; set; } = null!;
	public string UserName { get; set; } = null!;
	public string Email { get; set; } = null!;
	public string? ImageUrl { get; set; }
	public string? PhoneNumber { get; set; }
	public List<string>? Roles { get; set; }

}
