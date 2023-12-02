namespace Roaia.Core.Models;

public class Register
{
    [MaxLength(100)]
    public string FirstName { get; set; } = null!;

    [MaxLength(100)]
    public string LastName { get; set; } = null!;

    [MaxLength(50)]
    public string Username { get; set; } = null!;

    [MaxLength(128)]
    public string Email { get; set; } = null!;

    [MaxLength(256)]
    public string Password { get; set; }= null!;

    [MaxLength(20)]
    public string PhoneNumber { get; set; }= null!;
}
