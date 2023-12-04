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
