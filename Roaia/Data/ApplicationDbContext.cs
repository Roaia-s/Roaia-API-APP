﻿namespace Roaia.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Disease> Diseases { get; set; }
    public DbSet<Glasses> Glasses { get; set; }
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<DeviceToken> DeviceTokens { get; set; }
}