namespace Roaia.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<Disease> Diseases { get; set; }
    public DbSet<DeviceToken> DeviceTokens { get; set; }
    public DbSet<Glasses> Glasses { get; set; }
    public DbSet<AppNotification> Notifications { get; set; }
}