namespace Roaia.Core.Models.Dtos;

public class ModifyUserDto
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Password { get; set; } = null!;
    public IFormFile? ImageUrl { get; set; }

    public string? ImageName { get; set; }
}
