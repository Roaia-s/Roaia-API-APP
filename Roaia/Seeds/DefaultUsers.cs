namespace Roaia.Seeds;

public class DefaultUsers
{
    public static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
    {

        ApplicationUser admin = new()
        {
            UserName = "admin",
            Email = "roaiaofficial@gmail.com",
            FirstName = "Roaia",
            LastName = "Official",
            GlassesId = "4c215575-2575-4762-8f22-ace41c",
            ImageUrl = FileSettings.defaultImagesPath,
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
