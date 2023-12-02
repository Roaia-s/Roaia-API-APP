using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Roaia.Controllers;
[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService, IImageService imageService) : ControllerBase
{
    private readonly IAuthService _authService = authService;
    private readonly IImageService _imageService = imageService;

    // Route -> Register
    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromForm] Register model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (model.ImageUrl is not null)
        {
            var (isUploaded, errorMessage) = await _imageService.UploadAsync(model.ImageUrl, $"{model.Username}.png", $"/images/users", hasThumbnail: false);

            if (!isUploaded)
            {
                ModelState.AddModelError(nameof(model.ImageUrl), errorMessage);
                return BadRequest(ModelState);
            }
        }
        var result = await _authService.RegisterAsync(model);

        if (!result.IsAuthenticated)
            return BadRequest(result.Message);

        SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);

        return Ok(result);
    }

    // Route -> Login
    [HttpPost("token")]
    public async Task<IActionResult> GetTokenAsync([FromBody] TokenRequest model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.GetTokenAsync(model);

        if (!result.IsAuthenticated)
            return BadRequest(result.Message);

        if (!string.IsNullOrEmpty(result.RefreshToken))
            SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);

        return Ok(result);
    }

    // Route -> Assign User To Role
    [HttpPost("addrole")]
    public async Task<IActionResult> AddRoleAsync([FromBody] AddRole model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.AddRoleAsync(model);

        if (!string.IsNullOrEmpty(result))
            return BadRequest(result);

        return Ok(model);
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
    public async Task<IActionResult> RevokeToken([FromBody] RevokeToken model)
    {
        var token = model.Token ?? Request.Cookies["refreshToken"];

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
