using Stripe;
using System.Reflection;
using System.Text;

namespace Roaia.Extensions;

public static class DependancyInjection
{
    public static IServiceCollection AddRoaiaServices(this IServiceCollection services, WebApplicationBuilder builder)
    {
        services.Configure<JWT>(builder.Configuration.GetSection(nameof(JWT)));

        //Add DB
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));
        services.AddHttpClient();
        //
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAccountService, Services.AccountService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IAudioService, AudioService>();
        services.AddTransient<IImageService, ImageService>();
        services.AddTransient<IEmailSender, EmailSender>(); // Add EmailSender
        services.AddTransient<IEmailBodyBuilder, EmailBodyBuilder>();
        //Add Identity
        services
            .AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        //Configure Identity
        services.Configure<SecurityStampValidatorOptions>(options =>
            options.ValidationInterval = TimeSpan.Zero);

        services.Configure<DataProtectionTokenProviderOptions>(options => options.TokenLifespan = TimeSpan.FromMinutes(30));

        services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequiredLength = 8;
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(2);
        });

        // Add Authentication and JwtBearer
        services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = false;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = builder.Configuration["JWT:Issuer"],
                    ValidAudience = builder.Configuration["JWT:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"])),
                    ClockSkew = TimeSpan.Zero
                };
            });
        //add signalR
        services.AddSignalR();
        services.AddAutoMapper(Assembly.GetAssembly(typeof(MappingProfile)));

        services.Configure<MailSettings>(builder.Configuration.GetSection(nameof(MailSettings)));
        services.Configure<SpeechSettings>(builder.Configuration.GetSection(nameof(SpeechSettings)));
        services.Configure<OpenAISettings>(builder.Configuration.GetSection(nameof(OpenAISettings)));
        services.Configure<NotificationSettings>(builder.Configuration.GetSection(nameof(NotificationSettings)));
        services.Configure<StripeSettings>(builder.Configuration.GetSection(nameof(StripeSettings)));
        services.Configure<ApplicationSettings>(builder.Configuration.GetSection(nameof(ApplicationSettings)));
        StripeConfiguration.ApiKey = builder.Configuration.GetSection("StripeSettings:SecretKey").Get<string>();

        //
        FirebaseApp.Create(new AppOptions()
        {
            Credential = GoogleCredential.FromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "roaia-official-fcm.json")),
        });
        services.AddControllers();
        services.AddRazorPages();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddCors(options =>
            options.AddDefaultPolicy(policy =>
            policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin())
        /*.WithOrigins($"{builder.Configuration.GetSection("Application:AppDomain").Value}",
        $"{builder.Configuration.GetSection("Application:SubDomain").Value}")
        .AllowCredentials())*/
        );
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo()
            {
                Version = "v1",
                Title = "Roaia.Api",
                Description = "To Contact With ",
                Contact = new OpenApiContact()
                {
                    Name = "Shehab Lotfallah",
                    Email = "shehabw126@gmail.com",
                    Url = new Uri("https://zaap.bio/ShehabLotfallah")
                },
                License = new OpenApiLicense()
                {
                    Name = "MIT",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                },
                TermsOfService = new Uri("https://opensource.org/licenses/MIT")
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\""
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
               {
                  new OpenApiSecurityScheme()
                  {
                     Reference = new OpenApiReference()
                     {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                     },
                     Name = "Bearer",
                     In = ParameterLocation.Header
                  },
                  new List<string>()
               }
            });
        });

        services.AddMemoryCache();
        services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
        services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
        services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
        services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

        // Add Hangfire
        services.AddHangfire(x => x.UseSqlServerStorage(connectionString));
        services.AddHangfireServer();

        services.Configure<AuthorizationOptions>(options =>
        options.AddPolicy("AdminsOnly", policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireRole(AppRoles.Admin);
        }));

        // Add response compression
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<GzipCompressionProvider>();
            options.Providers.Add<BrotliCompressionProvider>();
        });

        return services;
    }
}
