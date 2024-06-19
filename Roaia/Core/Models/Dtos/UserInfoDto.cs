namespace Roaia.Core.Models.Dtos;

public class UserInfoDto
{
    public string? Message { get; set; }
    public string Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string BlindId { get; set; }
    public string? ImageUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public List<string>? Roles { get; set; }

}
