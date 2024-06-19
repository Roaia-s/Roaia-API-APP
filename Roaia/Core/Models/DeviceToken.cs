namespace Roaia.Core.Models;

public class DeviceToken
{
    public int Id { get; set; }
    public string Token { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.Now;
    public DateTime? LastUpdatedOn { get; set; }
    public string? GlassesId { get; set; }
    public Glasses? Glasses { get; set; }
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

}
