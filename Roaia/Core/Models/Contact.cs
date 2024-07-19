namespace Roaia.Core.Models;

public class Contact
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public int? Age { get; set; }
    public string? Relation { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsDeleted { get; set; }
    public string? GlassesId { get; set; }

    public Glasses? Glasses { get; set; }
}
