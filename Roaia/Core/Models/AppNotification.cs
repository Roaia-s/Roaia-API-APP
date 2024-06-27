namespace Roaia.Core.Models;

public class AppNotification
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public string? ImageUrl { get; set; }
    public string? AudioUrl { get; set; }
    public string GlassesId { get; set; }
    [JsonIgnore]
    public virtual Glasses? Glasses { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public bool IsRead { get; set; }
}
