
using Azure.Core;
using Humanizer;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Text.Encodings.Web;

namespace Roaia.Services;

public class AuthService(UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<JWT> jwt, IEmailSender emailSender, IEmailBodyBuilder emailBodyBuilder) : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;
    private readonly JWT _jwt = jwt.Value;
    private readonly IEmailSender _emailSender = emailSender;

    private readonly IEmailBodyBuilder _emailBodyBuilder = emailBodyBuilder;

    public async Task<AuthDto> RegisterAsync(RegisterDto dto)
    {
        if (await _userManager.FindByEmailAsync(dto.Email) is not null)
            return new AuthDto { Message = "Email is already registered!" };

        if (await _userManager.FindByNameAsync(dto.Username) is not null)
            return new AuthDto { Message = "Username is already registered!" };

        var user = new ApplicationUser
        {
            UserName = dto.Username,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PhoneNumber = dto.PhoneNumber,
            ImageUrl = $"/images/books/{dto.ImageName}"
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            var errors = string.Empty;

            foreach (var error in result.Errors)
                errors += $"{error.Description},";

            return new AuthDto { Message = errors };
        }

        await _userManager.AddToRoleAsync(user, "User");

        var jwtSecurityToken = await CreateJwtToken(user);

        var refreshToken = GenerateRefreshToken();
        user.RefreshTokens?.Add(refreshToken);
        await _userManager.UpdateAsync(user);

        return new AuthDto
        {
            Email = user.Email,
            //ExpiresOn = jwtSecurityToken.ValidTo,
            IsAuthenticated = true,
            Roles = new List<string> { "User" },
            Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
            Username = user.UserName,
            RefreshToken = refreshToken.Token,
            RefreshTokenExpiration = refreshToken.ExpiresOn
        };

    }

    public async Task<AuthDto> GetTokenAsync(TokenRequestDto dto)
    {
        AuthDto auth = new();
        var email = dto.Email.ToUpper();
        var user = await _userManager.Users
            .SingleOrDefaultAsync(u => (u.NormalizedUserName == email || u.NormalizedEmail == email) && !u.IsDeleted);

        if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            auth.Message = "Email or Password is incorrect!";
            return auth;
        }

        var jwtSecurityToken = await CreateJwtToken(user);
        var rolesList = await _userManager.GetRolesAsync(user);

        auth.IsAuthenticated = true;
        auth.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        auth.Email = user.Email;
        auth.Username = user.UserName;
        //auth.ExpiresOn = jwtSecurityToken.ValidTo;
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
            expires: DateTime.Now.AddDays(_jwt.DurationInDays),
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
        var roles = await _userManager.GetRolesAsync(user);
        auth.Roles = roles.ToList();
        auth.RefreshToken = newRefreshToken.Token;
        auth.RefreshTokenExpiration = newRefreshToken.ExpiresOn;

        return auth;
    }

    public async Task<bool> RevokeTokenAsync(string token)
    {
        var user = await _userManager.Users.SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));

        if (user == null)
            return false;

        var refreshToken = user.RefreshTokens.Single(t => t.Token == token);

        if (!refreshToken.IsActive)
            return false;

        refreshToken.RevokedOn = DateTime.UtcNow;

        await _userManager.UpdateAsync(user);

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
        var email = dto.Email.ToUpper();

        var user = await _userManager.Users
            .SingleOrDefaultAsync(u => (u.NormalizedUserName == email || u.NormalizedEmail == email) && !u.IsDeleted);

        if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            auth.Message = "Email or Password is incorrect!";
            return auth;
        }



        var jwtSecurityToken = await CreateJwtToken(user);
        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.PhoneNumber = dto.PhoneNumber;
        user.ImageUrl = $"/images/books/{dto.ImageName}";

        await _userManager.UpdateAsync(user);

        var rolesList = await _userManager.GetRolesAsync(user);
        auth.Message = "User Modfied Successfuly";
        auth.Email = user.Email;
        auth.IsAuthenticated = true;
        auth.Roles = rolesList.ToList();
        auth.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

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
            .SingleOrDefaultAsync(u => (u.NormalizedUserName == email || u.NormalizedEmail == email) && !u.IsDeleted);

        if (user is null || !await _userManager.CheckPasswordAsync(user, dto.OldPassword))
        {
            auth.Message = "Email or Password is incorrect!";
            return auth;
        }
        await _userManager.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword);

        var jwtSecurityToken = await CreateJwtToken(user);
        var rolesList = await _userManager.GetRolesAsync(user);
        auth.Message = "Password Changed Successfuly";
        auth.Email = user.Email;
        auth.IsAuthenticated = true;
        auth.Roles = rolesList.ToList();
        auth.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

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

    public async Task<string> SendResetPasswordEmailAsync(string email)
    {
        var errorMessage = string.Empty;
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            errorMessage = "This Email Does Not  Exist";
            return errorMessage;
        }

        // For more information on how to enable account confirmation and password reset please
        // visit https://go.microsoft.com/fwlink/?LinkID=532713
        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = $"https://localhost:44363/resetpassword?email={email}&token={code}";

        var body = _emailBodyBuilder.GetEmailBody(
        "https://res.cloudinary.com/devcreed/image/upload/v1668739431/icon-positive-vote-2_jcxdww.svg",
                $"Hey {user.FirstName},",
                "please click the below button to reset your password",
                $"{HtmlEncoder.Default.Encode(callbackUrl!)}",
                "Reset Password"
        );

        await _emailSender.SendEmailAsync(user.Email, "Reset Password", body);

        //user.ResetPasswordToken = emailToken;
        //user.ResetPasswordTokenExpiry = DateTime.Now.AddHours(1);
        //string from = _config["EmailSettings:From"];
            //var emailModel = new EmailModel(email, "Reset Password!", EmailBody.EmailStringBody(email, emailToken));
            //_emailService.SendEmail(emailModel);
        //await _userManager.UpdateAsync(user);
        return errorMessage;

    }

    public async Task<string> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
    {
        var errorMessage = string.Empty;
            //var newToken = resetPasswordDto.EmailToken.Replace(" ", "+");
        var user = await _userManager.Users.SingleOrDefaultAsync(x => x.Email == resetPasswordDto.Email);
        if (user == null)
        {
            errorMessage = "This User Does Not  Exist";
            return errorMessage;
        }
        if (resetPasswordDto.NewPassword != resetPasswordDto.ConfirmPassword)
        {
			errorMessage = "Password doesn't match its confirmation";
			return errorMessage;
		}
            //var token = user.ResetPasswordToken;
            //DateTime emailTokenExpiry = user.ResetPasswordTokenExpiry;
            //if (token != resetPasswordDto.EmailToken || emailTokenExpiry < DateTime.Now)
            //{
            //    errorMessage = "Invalid Reset Password Token";
            //    return errorMessage;
            //}

            //if (user.PasswordHash != null)
            //{
            //    await _userManager.RemovePasswordAsync(user);
            //}

            //await _userManager.AddPasswordAsync(user, resetPasswordDto.Password);
            //await _userManager.UpdateAsync(user);
        await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);
            //await _userManger.UpdateAsync(user);
        return errorMessage;
    }

    public async Task<ApplicationUser> FindByEmail(string email)
    {

        var user = await _userManager.Users
            .SingleOrDefaultAsync(u => (u.NormalizedUserName == email.ToUpper() || u.NormalizedEmail == email.ToUpper()) && !u.IsDeleted);

        return user;
    }

	public async Task<ApplicationUser> GetUserById(string id)
	{
		return await _userManager.FindByIdAsync(id);
	}

	public async Task<List<UserInfoDto>> GetUsersInfoAsync()
	{
		var users = new List<UserInfoDto>();
		var allusers = await _userManager.Users.ToListAsync();
		foreach (var user in allusers)
		{
			var userRoles = await _userManager.GetRolesAsync(user);
			var userinfo = new UserInfoDto()
			{
				Id = user.Id,
				FirstName = user.FirstName,
				LastName = user.LastName,
				Email = user.Email,
				UserName = user.UserName,
				PhoneNumber = user.PhoneNumber,
				Roles = userRoles.ToList(),
			};
			users.Add(userinfo);
		}
		return users;
	}

	public async Task<UserInfoDto> GetUserByIdAsync(string userId)
	{
		var userInfo = new UserInfoDto();
		var user = await _userManager.FindByIdAsync(userId);
		var userRoles = await _userManager.GetRolesAsync(user);
		userInfo.Id = userId;
		userInfo.UserName = user.UserName;
		userInfo.Email = user.Email;
		userInfo.FirstName = user.FirstName;
		userInfo.LastName = user.LastName;
		userInfo.PhoneNumber = user.PhoneNumber;
		userInfo.Roles = userRoles.ToList();

		return userInfo;
	}
}
