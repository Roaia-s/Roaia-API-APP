namespace Roaia.Seeds;

public class DefaultUsers
{
	public static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
	{

		ApplicationUser admin = new()
		{
			UserName = "admin",
			Email = "admin@roaia.com",
			FirstName = "Roaia",
			LastName = "Admin",
			EmailConfirmed = true
		};

		var user = await userManager.FindByEmailAsync(admin.Email);

		if (user is null)
		{
			await userManager.CreateAsync(admin, "Roaia832.Info");
			await userManager.AddToRoleAsync(admin, AppRoles.Admin);
		}
	}
}
