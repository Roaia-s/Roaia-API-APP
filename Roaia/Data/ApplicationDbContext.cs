using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Roaia.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
	{
	}

	public DbSet<Disease> Diseases { get; set; }
	public DbSet<Glasses> Glasses { get; set; }
	public DbSet<Contact> Contacts { get; set; }

}
