namespace Roaia.Core.Models;

public class Glasses
{
    public string Id { get; set; } = Guid.NewGuid().ToString().Substring(0, 30);
    [MaxLength(100)]
    public string? FullName { get; set; }
    public int? Age { get; set; }
    [MaxLength(15)]
    public string? Gender { get; set; }
    public string? ImageUrl { get; set; }

    public ICollection<Disease> Diseases { get; set; } = new List<Disease>();
}
