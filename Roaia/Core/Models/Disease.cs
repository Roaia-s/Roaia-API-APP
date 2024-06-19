namespace Roaia.Core.Models;

public class Disease
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string? Name { get; set; }
    public string? GlassesId { get; set; }
    public Glasses? Glasses { get; set; }
}
