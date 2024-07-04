using Microsoft.AspNetCore.WebUtilities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;

namespace Roaia.Services;

public class AuthService(UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager, ApplicationDbContext context,
    IOptions<JWT> jwt, IConfiguration configuration,
    IEmailSender emailSender, IEmailBodyBuilder emailBodyBuilder,
    IImageService imageService) : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;
    private readonly ApplicationDbContext _context = context;
    private readonly JWT _jwt = jwt.Value;

    private readonly IConfiguration _configuration = configuration;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly IEmailBodyBuilder _emailBodyBuilder = emailBodyBuilder;
    private readonly IImageService _imageService = imageService;

    public async Task<AuthDto> RegisterAsync(RegisterDto dto)
    {
        if (await _userManager.FindByEmailAsync(dto.Email) is not null)
            return new AuthDto { Message = "Email is already registered!" };

        if (await _userManager.FindByNameAsync(dto.Username) is not null)
            return new AuthDto { Message = "Username is already registered!" };

        // check if the phone number is unique
        if (dto.PhoneNumber is not null && await _userManager.Users.AnyAsync(u => u.PhoneNumber == dto.PhoneNumber))
            return new AuthDto { Message = "Phone Number is already registered!" };

        if (await _context.Glasses.FindAsync(dto.BlindId) is null)
            return new AuthDto { Message = "This Glasess Id Does Not Exist!" };

        if (dto.ImageUrl is not null)
        {
            var imageName = $"{Guid.NewGuid()}{Path.GetExtension(dto.ImageUrl.FileName)}";
            var (isUploaded, errorMessage) = await _imageService.UploadAsync(dto.ImageUrl,
                imageName,
                $"{FileSettings.usersImagesPath}", hasThumbnail: false);

            if (!isUploaded)
                return new AuthDto { Message = errorMessage };

            dto.ImageName = $"{FileSettings.usersImagesPath}/{imageName}";
        }

        var user = new ApplicationUser
        {
            UserName = dto.Username,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            GlassesId = dto.BlindId,
            PhoneNumber = dto.PhoneNumber,
            ImageUrl = !string.IsNullOrEmpty(dto.ImageName) ? dto.ImageName : $"{FileSettings.defaultImagesPath}",
            IsAgree = true,
            IsSubscribed = true,
            CreatedOn = DateTime.Now
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            var errors = string.Empty;

            foreach (var error in result.Errors)
                errors += $"{error.Description},";

            return new AuthDto { Message = errors };
        }

        var status = await SendOTPCodeAsync(dto.Email);
        if (!string.IsNullOrEmpty(status))
            return new AuthDto { Message = status };

        await _userManager.AddToRoleAsync(user, "User");

        var jwtSecurityToken = await CreateJwtToken(user);

        var refreshToken = GenerateRefreshToken();
        user.RefreshTokens?.Add(refreshToken);
        await _userManager.UpdateAsync(user);

        return new AuthDto
        {
            Email = user.Email,
            ExpiresOn = jwtSecurityToken.ValidTo,
            IsAuthenticated = true,
            Roles = new List<string> { "User" },
            Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
            Username = user.UserName,
            RefreshToken = refreshToken.Token,
            RefreshTokenExpiration = refreshToken.ExpiresOn,
            Message = "Email Send Successfully"
        };
    }

    public async Task<AuthDto> GetTokenAsync(TokenRequestDto dto)
    {
        AuthDto auth = new();
        var email = dto.Email.ToUpper();
        var user = await _userManager.Users
            .SingleOrDefaultAsync(u => (u.NormalizedUserName == email || u.NormalizedEmail == email || u.PhoneNumber == email) && !u.IsDeleted);

        if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            auth.Message = "Email or Password is incorrect!";
            return auth;
        }

        if (!user.EmailConfirmed)
        {
            auth.Message = "Please verify your email address!";
            return auth;
        }

        if (!dto.DeviceToken.IsNullOrEmpty())
        {
            //check if the device token is already exist
            var deviceToken = await _context.DeviceTokens.SingleOrDefaultAsync(t => t.Token == dto.DeviceToken);

            if (deviceToken is null)
            {
                deviceToken = new DeviceToken
                {
                    Token = dto.DeviceToken,
                    LastUpdatedOn = DateTime.Now,
                    GlassesId = user.GlassesId,
                    UserId = user.Id
                };
                _context.DeviceTokens.Add(deviceToken);
            }
            await _context.SaveChangesAsync();
        }

        var jwtSecurityToken = await CreateJwtToken(user);
        var rolesList = await _userManager.GetRolesAsync(user);

        auth.IsAuthenticated = true;
        auth.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        auth.Email = user.Email;
        auth.Username = user.UserName;
        auth.ExpiresOn = jwtSecurityToken.ValidTo;
        auth.Roles = rolesList.ToList();

        if (user.RefreshTokens.Any(t => t.IsActive))
        {
            var activeRefreshToken = user.RefreshTokens.FirstOrDefault(t => t.IsActive);
            auth.RefreshToken = activeRefreshToken.Token;
            auth.RefreshTokenExpiration = activeRefreshToken.ExpiresOn;
        }
        else
        {
            var refreshToken = GenerateRefreshToken();
            auth.RefreshToken = refreshToken.Token;
            auth.RefreshTokenExpiration = refreshToken.ExpiresOn;
            user.RefreshTokens.Add(refreshToken);
            await _userManager.UpdateAsync(user);
        }

        return auth;
    }

    //public async Task<AuthDto> GoogleLoginAsync(string idToken)
    //{
    //	AuthDto auth = new();
    //	var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);

    //	if (payload is null)
    //	{
    //		auth.Message = "Invalid Google Token";
    //		return auth;
    //	}

    //	var user = await _userManager.FindByEmailAsync(payload.Email);

    //	if (user is null)
    //	{
    //		user = new ApplicationUser
    //		{
    //			UserName = payload.Email,
    //			Email = payload.Email,
    //			FirstName = payload.GivenName,
    //			LastName = payload.FamilyName,
    //			ImageUrl = payload.Picture,
    //			IsAgree = true,
    //			CreatedOn = DateTime.Now
    //		};

    //		var result = await _userManager.CreateAsync(user);

    //		if (!result.Succeeded)
    //		{
    //			var errors = string.Empty;

    //			foreach (var error in result.Errors)
    //				errors += $"{error.Description},";

    //			auth.Message = errors;
    //			return auth;
    //		}

    //		await _userManager.AddToRoleAsync(user, "User");
    //	}

    //	var jwtSecurityToken = await CreateJwtToken(user);
    //	var rolesList = await _userManager.GetRolesAsync(user);

    //	auth.IsAuthenticated = true;
    //	auth.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
    //	auth.Email = user.Email;
    //	auth.Username = user.UserName;
    //	auth.ExpiresOn = jwtSecurityToken.ValidTo;
    //	auth.Roles = rolesList.ToList();

    //	if (user.RefreshTokens.Any(t => t.IsActive))
    //	{
    //		var activeRefreshToken = user.RefreshTokens.FirstOrDefault(t => t.IsActive);
    //		auth.RefreshToken = activeRefreshToken.Token;
    //		auth.RefreshTokenExpiration = activeRefreshToken.ExpiresOn;
    //	}
    //	else
    //	{
    //		var refreshToken = GenerateRefreshToken();
    //		auth.RefreshToken = refreshToken.Token;
    //		auth.RefreshTokenExpiration = refreshToken.ExpiresOn;
    //		user.RefreshTokens.Add(refreshToken);
    //		await _userManager.UpdateAsync(user);
    //	}

    //	return auth;
    //}

    //public async Task<AuthDto> FacebookLoginAsync(string accessToken)
    //{
    //	AuthDto auth = new();
    //	var fb = new FacebookClient(accessToken);
    //	dynamic response = await fb.GetTaskAsync("me", new { fields = "id, name, email, picture" });

    //	if (response is null)
    //	{
    //		auth.Message = "Invalid Facebook Token";
    //		return auth;
    //	}

    //	var user = await _userManager.FindByEmailAsync(response.email);

    //	if (user is null)
    //	{
    //		user = new ApplicationUser
    //		{
    //			UserName = response.email,
    //			Email = response.email,
    //			FirstName = response.name,
    //			LastName = response.name,
    //			ImageUrl = response.picture.data.url,
    //			IsAgree = true,
    //			CreatedOn = DateTime.Now
    //		};

    //		var result = await _userManager.CreateAsync(user);

    //		if (!result.Succeeded)
    //		{
    //			var errors = string.Empty;

    //			foreach (var error in result.Errors)
    //				errors += $"{error.Description},";

    //			auth.Message = errors;
    //			return auth;
    //		}

    //		await _userManager.AddToRoleAsync(user, "User");
    //	}

    //	var jwtSecurityToken = await CreateJwtToken(user);
    //	var rolesList = await _userManager.GetRolesAsync(user);

    //	auth.IsAuthenticated = true;
    //	auth.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
    //	auth.Email = user.Email;
    //	auth.Username = user.UserName;
    //	auth.ExpiresOn = jwtSecurityToken.ValidTo;
    //	auth.Roles = rolesList.ToList();

    //	if (user.RefreshTokens.Any(t => t.IsActive))
    //	{
    //		var activeRefreshToken = user.RefreshTokens.FirstOrDefault(t => t.IsActive);
    //		auth.RefreshToken = activeRefreshToken.Token;
    //		auth.RefreshTokenExpiration = activeRefreshToken.ExpiresOn;
    //	}
    //	else
    //	{
    //		var refreshToken = GenerateRefreshToken();
    //		auth.RefreshToken = refreshToken.Token;
    //		auth.RefreshTokenExpiration = refreshToken.ExpiresOn;
    //		user.RefreshTokens.Add(refreshToken);
    //		await _userManager.UpdateAsync(user);
    //	}

    //	return auth;
    //}

    //public async Task<AuthDto> TwitterLoginAsync(string accessToken, string accessTokenSecret)
    //{
    //	AuthDto auth = new();
    //	var twitter = new TwitterClient(accessToken, accessTokenSecret);
    //	var userResponse = await twitter.Users.GetAuthenticatedUserAsync();

    //	if (userResponse is null)
    //	{
    //		auth.Message = "Invalid Twitter Token";
    //		return auth;
    //	}

    //	var user = await _userManager.FindByEmailAsync(userResponse.Email);

    //	if (user is null)
    //	{
    //		user = new ApplicationUser
    //		{
    //			UserName = userResponse.Email,
    //			Email = userResponse.Email,
    //			FirstName = userResponse.Name,
    //			LastName = userResponse.Name,
    //			ImageUrl = userResponse.ProfileImageUrl,
    //			IsAgree = true,
    //			CreatedOn = DateTime.Now
    //		};

    //		var result = await _userManager.CreateAsync(user);

    //		if (!result.Succeeded)
    //		{
    //			var errors = string.Empty;

    //			foreach (var error in result.Errors)
    //				errors += $"{error.Description},";

    //			auth.Message = errors;
    //			return auth;
    //		}

    //		await _userManager.AddToRoleAsync(user, "User");
    //	}

    //	var jwtSecurityToken = await CreateJwtToken(user);
    //	var rolesList = await _userManager.GetRolesAsync(user);

    //	auth.IsAuthenticated = true;
    //	auth.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
    //	auth.Email = user.Email;
    //	auth.Username = user.UserName;
    //	auth.ExpiresOn = jwtSecurityToken.ValidTo;
    //	auth.Roles = rolesList.ToList();

    //	if (user.RefreshTokens.Any(t => t.IsActive))
    //	{
    //		var activeRefreshToken = user.RefreshTokens.FirstOrDefault(t => t.IsActive);
    //		auth.RefreshToken = activeRefreshToken.Token;
    //		auth.RefreshTokenExpiration = activeRefreshToken.ExpiresOn;
    //	}
    //	else
    //	{
    //		var refreshToken = GenerateRefreshToken();
    //		auth.RefreshToken = refreshToken.Token;
    //		auth.RefreshTokenExpiration = refreshToken.ExpiresOn;
    //		user.RefreshTokens.Add(refreshToken);
    //		await _userManager.UpdateAsync(user);
    //	}

    //	return auth;
    //}

    public async Task<string> AddRoleAsync(AddRoleDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId);

        if (user is null || !await _roleManager.RoleExistsAsync(dto.Role))
            return "Invalid user ID or Role";

        if (await _userManager.IsInRoleAsync(user, dto.Role))
            return "User already assigned to this role";

        var result = await _userManager.AddToRoleAsync(user, dto.Role);

        return result.Succeeded ? string.Empty : "Something went wrong";
    }

    private async Task<JwtSecurityToken> CreateJwtToken(ApplicationUser user)
    {
        var userClaims = await _userManager.GetClaimsAsync(user);
        var roles = await _userManager.GetRolesAsync(user);
        var roleClaims = new List<Claim>();

        foreach (var role in roles)
            roleClaims.Add(new Claim("roles", role));

        var claims = new[]
        {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("uid", user.Id)
            }
        .Union(userClaims)
        .Union(roleClaims);

        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

        var jwtSecurityToken = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(_jwt.DurationInMinutes),
            signingCredentials: signingCredentials);

        return jwtSecurityToken;
    }

    public async Task<AuthDto> RefreshTokenAsync(string token)
    {
        var auth = new AuthDto();

        var user = await _userManager.Users.SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));

        if (user == null)
        {
            auth.Message = "Invalid token";
            return auth;
        }

        var refreshToken = user.RefreshTokens.Single(t => t.Token == token);

        if (!refreshToken.IsActive)
        {
            auth.Message = "Inactive token";
            return auth;
        }

        refreshToken.RevokedOn = DateTime.UtcNow;

        var newRefreshToken = GenerateRefreshToken();
        user.RefreshTokens.Add(newRefreshToken);
        await _userManager.UpdateAsync(user);

        var jwtToken = await CreateJwtToken(user);
        auth.IsAuthenticated = true;
        auth.Token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
        auth.Email = user.Email;
        auth.Username = user.UserName;
        auth.ExpiresOn = jwtToken.ValidTo;
        var roles = await _userManager.GetRolesAsync(user);
        auth.Roles = roles.ToList();
        auth.RefreshToken = newRefreshToken.Token;
        auth.RefreshTokenExpiration = newRefreshToken.ExpiresOn;

        return auth;
    }

    public async Task<bool> RevokeTokenAsync(RevokeTokenDto dto)
    {
        var user = await _userManager.Users.SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == dto.Token));

        if (user == null)
            return false;

        var refreshToken = user.RefreshTokens.Single(t => t.Token == dto.Token);

        if (!refreshToken.IsActive)
            return false;

        refreshToken.RevokedOn = DateTime.UtcNow;

        await _userManager.UpdateAsync(user);

        if (dto.DeviceToken is not null)
        {
            var device = await _context.DeviceTokens.SingleOrDefaultAsync(t => t.Token == dto.DeviceToken && t.UserId == user.Id);
            if (device is not null)
            {
                _context.DeviceTokens.Remove(device);
                await _context.SaveChangesAsync();
            }
        }
        return true;
    }

    private RefreshToken GenerateRefreshToken()
    {
        var randomNumber = new byte[32];

        using var generator = new RNGCryptoServiceProvider();

        generator.GetBytes(randomNumber);

        return new RefreshToken
        {
            Token = Convert.ToBase64String(randomNumber),
            ExpiresOn = DateTime.UtcNow.AddDays(10),
            CreatedOn = DateTime.UtcNow
        };
    }

    public async Task<AuthDto> ModfiyUserAsync(ModifyUserDto dto)
    {
        AuthDto auth = new();
        var email = dto.Email.ToUpper() ?? dto.UserName.ToUpper();

        var user = await _userManager.Users
            .SingleOrDefaultAsync(u => (u.NormalizedUserName == email || u.NormalizedEmail == email || u.PhoneNumber == email) && !u.IsDeleted);

        if (user is null)
        {
            auth.Message = "Email or username is incorrect!";
            return auth;
        }

        if (dto.ImageUrl is not null)
        {
            var imageName = $"{Guid.NewGuid()}{Path.GetExtension(dto.ImageUrl.FileName)}";
            var (isUploaded, errorMessage) = await _imageService.UploadAsync(dto.ImageUrl,
                imageName,
                $"{FileSettings.usersImagesPath}", hasThumbnail: false);

            if (!isUploaded)
            {
                auth.Message = errorMessage;
                return auth;
            }

            //to do delete old image    Done ☻
            if (!string.IsNullOrEmpty(user.ImageUrl))
                _imageService.Delete(user.ImageUrl);

            user.ImageUrl = $"{FileSettings.usersImagesPath}/{imageName}";
        }

        var jwtSecurityToken = await CreateJwtToken(user);

        // check phone number is unique except the current user
        if (dto.PhoneNumber is not null && await _userManager.Users.AnyAsync(u => u.PhoneNumber == dto.PhoneNumber && u.Id != user.Id))
        {
            auth.Message = "Phone Number is already registered!";
            return auth;
        }
        else
        {
            // check if the phone number is changed
            if (user.PhoneNumber != dto.PhoneNumber)
            {
                user.PhoneNumber = dto.PhoneNumber;
                user.PhoneNumberConfirmed = true;
            }
        }


        user.FirstName = dto.FirstName ?? user.FirstName;
        user.LastName = dto.LastName ?? user.LastName;
        user.UserName = dto.UserName ?? user.UserName;
        user.LastUpdatedOn = DateTime.Now;
        await _userManager.UpdateAsync(user);


        var rolesList = await _userManager.GetRolesAsync(user);

        auth.Message = "User Modfied Successfuly";
        auth.IsAuthenticated = true;
        auth.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        auth.Email = user.Email;
        auth.Username = user.UserName;
        auth.ExpiresOn = jwtSecurityToken.ValidTo;
        auth.Roles = rolesList.ToList();

        if (user.RefreshTokens.Any(t => t.IsActive))
        {
            var activeRefreshToken = user.RefreshTokens.FirstOrDefault(t => t.IsActive);
            auth.RefreshToken = activeRefreshToken.Token;
            auth.RefreshTokenExpiration = activeRefreshToken.ExpiresOn;
        }
        else
        {
            var refreshToken = GenerateRefreshToken();
            auth.RefreshToken = refreshToken.Token;
            auth.RefreshTokenExpiration = refreshToken.ExpiresOn;
            user.RefreshTokens.Add(refreshToken);
            await _userManager.UpdateAsync(user);
        }
        return auth;

    }

    public async Task<AuthDto> ChangePasswordAsync(ChangePasswordDto dto)
    {
        AuthDto auth = new();
        var email = dto.Email.ToUpper();

        var user = await _userManager.Users
            .SingleOrDefaultAsync(u => (u.NormalizedUserName == email || u.NormalizedEmail == email || u.PhoneNumber == email) && !u.IsDeleted);

        if (user is null)
        {
            auth.Message = "Email or username is incorrect!";
            return auth;
        }

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

        if (!result.Succeeded)
        {
            auth.Message = "Old Password is incorrect!";
            return auth;
        }

        if (dto.NewPassword != dto.ConfirmPassword)
        {
            auth.Message = "Password doesn't match its confirmation";
            return auth;
        }

        var jwtSecurityToken = await CreateJwtToken(user);
        var rolesList = await _userManager.GetRolesAsync(user);

        auth.Message = "Password Changed Successfuly";
        auth.IsAuthenticated = true;
        auth.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        auth.Email = user.Email;
        auth.Username = user.UserName;
        auth.ExpiresOn = jwtSecurityToken.ValidTo;
        auth.Roles = rolesList.ToList();

        if (user.RefreshTokens.Any(t => t.IsActive))
        {
            var activeRefreshToken = user.RefreshTokens.FirstOrDefault(t => t.IsActive);
            auth.RefreshToken = activeRefreshToken.Token;
            auth.RefreshTokenExpiration = activeRefreshToken.ExpiresOn;
        }
        else
        {
            var refreshToken = GenerateRefreshToken();
            auth.RefreshToken = refreshToken.Token;
            auth.RefreshTokenExpiration = refreshToken.ExpiresOn;
            user.RefreshTokens.Add(refreshToken);
            user.LastUpdatedOn = DateTime.Now;
            await _userManager.UpdateAsync(user);
        }
        return auth;
    }

    public async Task<string> SendOTPCodeAsync(string Email)
    {
        var errorMessage = string.Empty;
        var email = Email.ToUpper();

        var user = await _userManager.Users
            .SingleOrDefaultAsync(u => (u.NormalizedUserName == email || u.NormalizedEmail == email || u.PhoneNumber == email) && !u.IsDeleted);

        if (user == null)
        {
            errorMessage = "This Email Does Not  Exist";
            return errorMessage;
        }

        // For more information on how to enable account confirmation and password reset please
        // visit https://go.microsoft.com/fwlink/?LinkID=532713

        Random rnd = new();
        var otpCode = rnd.Next(100000, 999999);

        if (!user.EmailConfirmed)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = $"{_configuration.GetSection("Application:AppDomain").Value}/api/Auth/verifyEmail?email={user.Email}&token={code}";

            var body = _emailBodyBuilder.GetEmailBody(
                "https://res.cloudinary.com/shehablotfallah/image/upload/v1699045725/icon-positive-vote-1_rdexez_wyxbpc.png",
                $"Hey {user.FirstName}, thanks for joining us!",
                $"To verify your email address, please use your OTP within <span style=\"color:#181C32;\">5 minutes</span><br/><br/> <b style=\"border-radius: 7px; padding: 12px 30px 12px 30px; border: 1px solid #1877f2; background: #e7f3ff; color: #181C32; font-size: 20px; font-weight:900\">{otpCode}</b><br/><br/> or click the below button to confirm your email",
                $"{HtmlEncoder.Default.Encode(callbackUrl!)}",
                $" Do not share this OTP with anyone.<br /> Roaia takes your account security very seriously. Roaia Customer Service will never ask you to disclose or verify your Roaia password, OTP, credit card, or banking account number. If you receive a suspicious email with a link to update your account information, do not click on the link instead, report the email to Roaia for investigation.<br /><br />Thank you!",
                "Confirm Email"
                );

            BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(user.Email!, $"OTP-{otpCode} is your Roaia confirmation code", body));

        }
        else
        {
            // For more information on how to enable account confirmation and password reset please
            // visit https://go.microsoft.com/fwlink/?LinkID=532713
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = $"{_configuration.GetSection("Application:AppDomain").Value}/ResetPassword?email={user.Email}&token={code}";

            var body = _emailBodyBuilder.GetEmailBody(
                "https://res.cloudinary.com/shehablotfallah/image/upload/v1707187426/icon-positive-vote-2_jcxdww_lbjawn_l8haqf.png",
                $"Hey {user.FirstName},",
                $"To resetting your password, please use your OTP within <span style=\"color:#181C32;\">5 minutes</span><br/><br/> <b style=\"border-radius: 7px; padding: 12px 30px 12px 30px; border: 1px solid #1877f2; background: #e7f3ff; color:#181C32; font-size: 20px; font-weight:900\">{otpCode}</b><br/><br/> or click the below button to reset your password",
                $"{HtmlEncoder.Default.Encode(callbackUrl!)}",
                $" Do not share this OTP with anyone.<br /> Roaia takes your account security very seriously. Roaia Customer Service will never ask you to disclose or verify your Roaia password, OTP, credit card, or banking account number. If you receive a suspicious email with a link to update your account information, do not click on the link instead, report the email to Roaia for investigation.<br /><br />Thank you!",
                "Reset Password"
            );

            BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(user.Email!, $"OTP-{otpCode} is your Roaia reset password code", body));
        }

        user.OtpCode = otpCode.ToString();
        user.OtpCodeExpiry = DateTime.UtcNow.AddMinutes(5);
        await _userManager.UpdateAsync(user);

        return errorMessage;
    }

    public async Task<string> SendMailNewsAsync(MailNewsDto dto)
    {
        var errorMessage = string.Empty;

        var emails = await _userManager.Users.Where(u => !(u.IsDeleted && u.IsSubscribed)).Select(u => u.Email).ToListAsync();

        if (emails.Count == 0)
            return errorMessage = "No Users Exist";

        foreach (var email in emails)
        {
            BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(email!, dto.Subject, dto.HtmlMessage));
        }

        return errorMessage;
    }

    // unsubscribe from mail news
    public async Task<string> UnsubscribeMailNewsAsync(string email)
    {

        var errorMessage = string.Empty;
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
            return errorMessage = "This User Does Not  Exist";

        user.IsSubscribed = false;
        await _userManager.UpdateAsync(user);

        return errorMessage;
    }

    // verify email
    public async Task<string> VerifyEmailAsync(string email, string token)
    {
        var errorMessage = string.Empty;
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null || user.IsDeleted)
        {
            errorMessage = "This User Does Not  Exist";
            return errorMessage;
        }

        var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        var result = await _userManager.ConfirmEmailAsync(user, code);

        if (!result.Succeeded)
        {
            errorMessage = "Invalid Token";
            return errorMessage;
        }
        //var body = _emailBodyBuilder.GetEmailBody();
        //return body;
        return errorMessage;
    }

    public async Task<string> OtbVerificationAsync(OTPVerificationDto dto)
    {
        var errorMessage = string.Empty;
        var email = dto.Email.ToUpper();

        var user = await _userManager.Users
            .SingleOrDefaultAsync(u => (u.NormalizedUserName == email || u.NormalizedEmail == email || u.PhoneNumber == email) && !u.IsDeleted);

        if (user is null)
        {
            errorMessage = "This User Does Not  Exist";
            return errorMessage;
        }

        if (!(user.OtpCode == dto.OtpCode && user.OtpCodeExpiry > DateTime.UtcNow))
        {
            errorMessage = "Invalid Token";
            return errorMessage;
        }

        if (user.EmailConfirmed == false)
        {
            user.EmailConfirmed = !user.EmailConfirmed;
            await _userManager.UpdateAsync(user);
        }

        return errorMessage;

    }

    public async Task<AuthDto> ResetPasswordAsync(ResetPasswordDto dto)
    {
        AuthDto auth = new();

        var email = dto.Email.ToUpper();

        var user = await _userManager.Users
            .SingleOrDefaultAsync(u => (u.NormalizedUserName == email || u.NormalizedEmail == email || u.PhoneNumber == email) && !u.IsDeleted);

        if (user is null)
            return new AuthDto { Message = "This User Does Not  Exist" };

        if (dto.NewPassword != dto.ConfirmPassword)
            return new AuthDto { Message = "Password doesn't match its confirmation" };

        // check if length of dto.token is greater than 7
        if (dto.Token?.Length > 7)
        {
            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(dto.Token));
            var result = await _userManager.ResetPasswordAsync(user, code, dto.NewPassword);

            if (!result.Succeeded)
                return new AuthDto { Message = "Invalid Token" };
        }
        else if (user.PasswordHash != null)
        {
            await _userManager.RemovePasswordAsync(user);
            await _userManager.AddPasswordAsync(user, dto.NewPassword);
        }

        user.LastUpdatedOn = DateTime.Now;
        await _userManager.UpdateAsync(user);
        var jwtSecurityToken = await CreateJwtToken(user);
        var rolesList = await _userManager.GetRolesAsync(user);

        auth.IsAuthenticated = true;
        auth.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        auth.Email = user.Email;
        auth.Username = user.UserName;
        auth.ExpiresOn = jwtSecurityToken.ValidTo;
        auth.Roles = rolesList.ToList();

        if (user.RefreshTokens.Any(t => t.IsActive))
        {
            var activeRefreshToken = user.RefreshTokens.FirstOrDefault(t => t.IsActive);
            auth.RefreshToken = activeRefreshToken.Token;
            auth.RefreshTokenExpiration = activeRefreshToken.ExpiresOn;
        }
        else
        {
            var refreshToken = GenerateRefreshToken();
            auth.RefreshToken = refreshToken.Token;
            auth.RefreshTokenExpiration = refreshToken.ExpiresOn;
            user.RefreshTokens.Add(refreshToken);
            user.LastUpdatedOn = DateTime.Now;
            await _userManager.UpdateAsync(user);
        }
        return auth;
    }

    public async Task<List<UserInfoDto>?> GetUsersInfoAsync()
    {
        var users = new List<UserInfoDto>();
        var allUsers = await _userManager.Users.AsNoTracking().ToListAsync();

        if (allUsers.Count == 0)
            return null;

        foreach (var user in allUsers)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            UserInfoDto userInfo = new();

            userInfo.Id = user.Id;
            userInfo.FirstName = user.FirstName;
            userInfo.LastName = user.LastName;
            userInfo.Email = user.Email;
            userInfo.UserName = user.UserName;
            userInfo.PhoneNumber = user.PhoneNumber;
            userInfo.ImageUrl = $"{_configuration.GetSection("Application:AppDomain").Value}{user.ImageUrl}";
            userInfo.BlindId = user.GlassesId;
            userInfo.Roles = userRoles.ToList();

            users.Add(userInfo);
        }
        return users;
    }

    public async Task<UserInfoDto> GetUserByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return new UserInfoDto { Message = "This User Does Not  Exist" };

        var userRoles = await _userManager.GetRolesAsync(user!);
        UserInfoDto userInfo = new();

        userInfo.Id = userId;
        userInfo.UserName = user.UserName;
        userInfo.Email = user.Email;
        userInfo.FirstName = user.FirstName;
        userInfo.LastName = user.LastName;
        userInfo.PhoneNumber = user.PhoneNumber;
        userInfo.ImageUrl = $"{_configuration.GetSection("Application:AppDomain").Value}{user.ImageUrl}";
        userInfo.BlindId = user.GlassesId;
        userInfo.Roles = userRoles.ToList();

        return userInfo;
    }
}
