using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;

namespace Roaia.Controllers;
[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService, IImageService imageService) : ControllerBase
{
    private readonly IAuthService _authService = authService;
    private readonly IImageService _imageService = imageService;

    // Route -> Register
    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromForm] RegisterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);


        if (dto.ImageUrl is not null)
        {
            var imageName = $"{Guid.NewGuid()}{Path.GetExtension(dto.ImageUrl.FileName)}";
            var (isUploaded, errorMessage) = await _imageService.UploadAsync(dto.ImageUrl,
                imageName,
                $"/images/users", hasThumbnail: false);

            if (!isUploaded)
            {
                ModelState.AddModelError(nameof(dto.ImageUrl), errorMessage);
                return BadRequest(ModelState);
            }

            dto.ImageName = imageName;
        }
        var result = await _authService.RegisterAsync(dto);

        if (!result.IsAuthenticated)
            return BadRequest(result.Message);

        SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);

        return Ok(result);
    }

    // Route -> Login
    [HttpPost("login")]
    public async Task<IActionResult> GetTokenAsync([FromBody] TokenRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.GetTokenAsync(dto);

        if (!result.IsAuthenticated)
            return BadRequest(result.Message);

        if (!string.IsNullOrEmpty(result.RefreshToken))
            SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);

        return Ok(result);
    }

    [HttpPatch("modifyUser")]
    public async Task<IActionResult> ModifiyUserAsync([FromForm] ModifyUserDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _authService.FindByEmail(dto.Email);

        if (dto.ImageUrl is not null)
        {
            var oldImagePath = user.ImageUrl;
            var imageName = $"{Guid.NewGuid()}{Path.GetExtension(dto.ImageUrl.FileName)}";
            var (isUploaded, errorMessage) = await _imageService.UploadAsync(dto.ImageUrl,
                imageName,
                $"/images/users", hasThumbnail: false);

            if (!isUploaded)
            {
                ModelState.AddModelError(nameof(dto.ImageUrl), errorMessage);
                return BadRequest(ModelState);
            }

            dto.ImageName = imageName;
            //to do delete old image    Done ☻
            _imageService.Delete(oldImagePath);
        }

        var result = await _authService.ModfiyUserAsync(dto);
        if (!result.IsAuthenticated)
            return BadRequest(result.Message);

        if (!string.IsNullOrEmpty(result.RefreshToken))
            SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);

        return Ok(result);
    }

    [HttpPatch("changePassword")]
    public async Task<IActionResult> ChangePasswordAsync(ChangePasswordDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.ChangePasswordAsync(dto);
        if (!result.IsAuthenticated)
            return BadRequest(result.Message);

        if (!string.IsNullOrEmpty(result.RefreshToken))
            SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);

        return Ok(result);
    }

    [HttpGet("get-users-list")]
    public async Task<IActionResult> GetUsersInfo()
    {
        var result = await _authService.GetUsersInfoAsync();
        if (result == null)
        {
            return NotFound("We Have Not Users Yet");
        }
        return Ok(result);
    }

    [HttpGet("get-user-by-id/{userId}")]
    public async Task<IActionResult> GetUserByIdAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest(new { message = "UserId Is Required" });
        }

        var result = await _authService.GetUserByIdAsync(userId);
        if (result == null)
        {
            return NotFound(new { message = "User Not Found" });
        }
        return Ok(result);
    }

    [HttpPost("forgetPassword/{email}")]
    public async Task<IActionResult> SendResetPasswordEmailAsync(string email)
    {
        if (string.IsNullOrEmpty(email))
            return BadRequest("Email Is Required");
        var result = await _authService.SendResetPasswordEmailAsync(email);

        if (!string.IsNullOrEmpty(result))
            return BadRequest(result);

        return Ok(new { message = "Email Send Successfully" });

    }

    [HttpPost("resetPassword")]
    public async Task<IActionResult> ResetPasswordAsync([FromForm]ResetPasswordDto resetPasswordDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.ResetPasswordAsync(resetPasswordDto);
        if (!string.IsNullOrEmpty(result))
            return BadRequest(result);

        return Ok(new { message = "Password Reset Successfully" });
    }
    // Route -> Assign User To Role
    [HttpPost("addRole")]
    public async Task<IActionResult> AddRoleAsync([FromBody] AddRoleDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.AddRoleAsync(dto);

        if (!string.IsNullOrEmpty(result))
            return BadRequest(result);

        return Ok(dto);
    }

    [HttpGet("refreshToken")]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];

        var result = await _authService.RefreshTokenAsync(refreshToken);

        if (!result.IsAuthenticated)
            return BadRequest(result);

        SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);

        return Ok(result);
    }

    [HttpPost("revokeToken")]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenDto dto)
    {
        var token = dto.Token ?? Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(token))
            return BadRequest("Token is required!");

        var result = await _authService.RevokeTokenAsync(token);

        if (!result)
            return BadRequest("Token is invalid!");

        return Ok();
    }

    // Append The Refresh Token To Response Cookie
    private void SetRefreshTokenInCookie(string refreshToken, DateTime expires)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = expires.ToLocalTime(),
            Secure = true,
            IsEssential = true,
            SameSite = SameSiteMode.None
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }

}
