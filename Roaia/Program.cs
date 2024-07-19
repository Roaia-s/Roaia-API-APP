using Serilog;
using Serilog.Context;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRoaiaServices(builder);

//Add Serilog
Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();

builder.Host.UseSerilog();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseIpRateLimiting();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();

using var scope = scopeFactory.CreateScope();

var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

await DefaultRoles.SeedAsync(roleManager);
await DefaultUsers.SeedAdminUserAsync(userManager);

//hangfire
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    DashboardTitle = "Roaia Dashboard",
    //IsReadOnlyFunc = (DashboardContext context) => true,
    Authorization = new IDashboardAuthorizationFilter[]
    {
        new HangfireAuthorizationFilter("AdminsOnly")
    }
});

app.UseWhen(context =>
    !(context.Request.Headers.ContainsKey("Authorization") ||
    context.Request.Cookies.Any() ||
    context.Response.Headers.ContainsKey("Cookie")),
    appBuilder =>
    {
        appBuilder.UseResponseCompression();
    });

app.Use(async (context, next) =>
{
    LogContext.PushProperty("UserId", context.User.FindFirst(ClaimTypes.Email)?.Value);
    LogContext.PushProperty("UserName", context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

    await next();
});

app.UseSerilogRequestLogging();

app.MapRazorPages();
app.MapHub<GPSHub>("/gps-Hub");
app.MapControllers();
app.Run();