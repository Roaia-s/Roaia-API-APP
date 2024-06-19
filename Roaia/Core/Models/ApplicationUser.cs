namespace Roaia.Core.Models;

[Index(nameof(Email), IsUnique = true)]
[Index(nameof(UserName), IsUnique = true)]
[Index(nameof(PhoneNumber), IsUnique = true)]
public class ApplicationUser : IdentityUser
{
    [MaxLength(100)]
    public string FirstName { get; set; } = null!;

    [MaxLength(100)]
    public string LastName { get; set; } = null!;

    public string? GlassesId { get; set; }

    public Glasses? Glasses { get; set; }

    public bool IsAgree { get; set; }

    public bool IsDeleted { get; set; }
    public bool IsSubscribed { get; set; }

    public DateTime CreatedOn { get; set; } = DateTime.Now;

    public DateTime? LastUpdatedOn { get; set; }

    public List<RefreshToken>? RefreshTokens { get; set; }

    public string? ImageUrl { get; set; }

    public string? OtpCode { get; set; }

    public DateTime OtpCodeExpiry { get; set; }

}
