namespace Roaia.Core.Models;

[Index(nameof(Email), IsUnique = true)]
[Index(nameof(UserName), IsUnique = true)]
public class ApplicationUser : IdentityUser
{
    [MaxLength(100)]
    public string FirstName { get; set; } = null!;

    [MaxLength(100)]
    public string LastName { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public DateTime CreatedOn { get; set; } = DateTime.Now;

    public DateTime? LastUpdatedOn { get; set; }

    public List<RefreshToken>? RefreshTokens { get; set; }

    public string? ImageUrl { get; set; }

    public string? ResetPasswordToken { get; set; }

    public DateTime ResetPasswordTokenExpiry { get; set; }

}
