using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

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

		if (await _context.Glasses.FindAsync(dto.BlindId) is null)
			return new AuthDto { Message = "This Id Does Not  Exist!" };

		if (dto.ImageUrl is not null)
		{
			var imageName = $"{Guid.NewGuid()}{Path.GetExtension(dto.ImageUrl.FileName)}";
			var (isUploaded, errorMessage) = await _imageService.UploadAsync(dto.ImageUrl,
				imageName,
				$"/images/users", hasThumbnail: false);

			if (!isUploaded)
				return new AuthDto { Message = errorMessage };

			dto.ImageName = $"/images/users/{imageName}";
		}

		var user = new ApplicationUser
		{
			UserName = dto.Username,
			Email = dto.Email,
			FirstName = dto.FirstName,
			LastName = dto.LastName,
			GlassesId = dto.BlindId,
			ImageUrl = !string.IsNullOrEmpty(dto.ImageName) ? dto.ImageName : $"/images/avatar.png",
			IsAgree = true,
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
			.SingleOrDefaultAsync(u => (u.NormalizedUserName == email || u.NormalizedEmail == email) && !u.IsDeleted);

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

		if (dto.ImageUrl is not null)
		{
			var imageName = $"{Guid.NewGuid()}{Path.GetExtension(dto.ImageUrl.FileName)}";
			var (isUploaded, errorMessage) = await _imageService.UploadAsync(dto.ImageUrl,
				imageName,
				$"/images/users", hasThumbnail: false);

			if (!isUploaded)
			{
				auth.Message = errorMessage;
				return auth;
			}

			//to do delete old image    Done ☻
			if (!string.IsNullOrEmpty(user.ImageUrl))
				_imageService.Delete(user.ImageUrl);

			user.ImageUrl = $"/images/users/{imageName}";
		}

		var jwtSecurityToken = await CreateJwtToken(user);
		user.FirstName = dto.FirstName;
		user.LastName = dto.LastName;
		user.PhoneNumber = dto.PhoneNumber;
		user.LastUpdatedOn = DateTime.Now;

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
			user.LastUpdatedOn = DateTime.Now;
			await _userManager.UpdateAsync(user);
		}
		return auth;
	}

	public async Task<string> SendOTPCodeAsync(string email)
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

		Random rnd = new();
		var otpCode = rnd.Next(100000, 999999);

		if (!user.EmailConfirmed)
		{
			var body = _emailBodyBuilder.GetEmailBody(
				"https://res.cloudinary.com/shehablotfallah/image/upload/v1699045725/icon-positive-vote-1_rdexez_wyxbpc.png",
				$"Hey {user.FirstName},",
				$"<span>To verify your email address, please use the following One Time Password (OTP): </span><br/><br/> <b style=\"color:#181C32; font-size: 20px; font-weight:900\"><mark>{otpCode}</mark></b><br/> <span>Please enter it within <mark> 5 minutes </mark>.</span>"
			);

			await _emailSender.SendEmailAsync(user.Email!, "Verify your email address", body);

		}
		else
		{
			var body = _emailBodyBuilder.GetEmailBody(
				"https://res.cloudinary.com/shehablotfallah/image/upload/v1707187426/icon-positive-vote-2_jcxdww_lbjawn_l8haqf.png",
				$"Hey {user.FirstName},",
				$"<span>Your OTP for resetting your password is </span><br/><br/> <b style=\"color:#181C32; font-size: 20px; font-weight:900\"><mark>{otpCode}</mark></b><br/> <span>Please enter it within <mark> 5 minutes </mark>.</span>"
			);

			await _emailSender.SendEmailAsync(user.Email!, "Reset Password", body);
		}

		user.OtpCode = otpCode.ToString();
		user.OtpCodeExpiry = DateTime.UtcNow.AddMinutes(4);
		await _userManager.UpdateAsync(user);

		return errorMessage;
	}

	public async Task<string> OtbVerificationAsync(OTPVerificationDto dto)
	{
		var errorMessage = string.Empty;
		var user = await _userManager.FindByEmailAsync(dto.Email);
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

	public async Task<string> ResetPasswordAsync(ResetPasswordDto dto)
	{
		var errorMessage = string.Empty;
		var user = await _userManager.FindByEmailAsync(dto.Email);

		if (user == null)
		{
			errorMessage = "This User Does Not  Exist";
			return errorMessage;
		}
		if (dto.NewPassword != dto.ConfirmPassword)
		{
			errorMessage = "Password doesn't match its confirmation";
			return errorMessage;
		}


		if (user.PasswordHash != null)
			await _userManager.RemovePasswordAsync(user);

		user.LastUpdatedOn = DateTime.Now;
		await _userManager.AddPasswordAsync(user, dto.NewPassword);
		await _userManager.UpdateAsync(user);

		return errorMessage;
	}

	public async Task<List<UserInfoDto>> GetUsersInfoAsync()
	{
		var users = new List<UserInfoDto>();
		var allUsers = await _userManager.Users.ToListAsync();
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
