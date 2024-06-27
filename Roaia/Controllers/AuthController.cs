namespace Roaia.Controllers;
[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService, IConfiguration configuration) : ControllerBase
{
    private readonly IAuthService _authService = authService;
    private readonly IConfiguration _configuration = configuration;

    // Route -> Register
    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromForm] RegisterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

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

    // Route -> Modify User
    [HttpPut("modifyUser")]
    public async Task<IActionResult> ModifiyUserAsync([FromForm] ModifyUserDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.ModfiyUserAsync(dto);
        if (!result.IsAuthenticated)
            return BadRequest(result.Message);

        if (!string.IsNullOrEmpty(result.RefreshToken))
            SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);

        return Ok(result);
    }

    // Route -> changePassword
    [HttpPut("changePassword")]
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

    // Route -> GetUsersInfo
    [HttpGet("get-users-list")]
    public async Task<IActionResult> GetUsersInfo()
    {
        var result = await _authService.GetUsersInfoAsync();
        if (result is null)
            return NotFound("We Have Not Users Yet");

        return Ok(result);
    }

    // Route -> GetUserById
    [HttpGet("get-user-by-id/{userId}")]
    public async Task<IActionResult> GetUserByIdAsync(string userId)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.GetUserByIdAsync(userId);
        if (result.Message is not null)
            return NotFound(new { message = result.Message });

        return Ok(result);
    }

    // Route -> unsubscribeMailNews
    [HttpPost("unsubscribeMailNews/{email}")]
    public async Task<IActionResult> UnsubscribeMailNewsAsync(string email)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.UnsubscribeMailNewsAsync(email);
        if (!string.IsNullOrEmpty(result))
            return BadRequest(result);

        return Ok(new { message = "Unsubscribed Successfully" });
    }


    // Route -> Send OTP Code
    [HttpPost("SendOTPCode/{email}")]
    public async Task<IActionResult> SendOTPToEmailAsync(string email)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.SendOTPCodeAsync(email);

        if (!string.IsNullOrEmpty(result))
            return BadRequest(result);

        return Ok(new { message = "Email Send Successfully" });

    }

    // Route -> verify Email
    [HttpGet("verifyEmail")]
    public async Task<IActionResult> VerifyEmailAsync([FromQuery] string email, string token)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.VerifyEmailAsync(email, token);
        if (!string.IsNullOrEmpty(result))
            return Redirect($"{_configuration.GetSection("Application:AppDomain").Value}/BadRequest");

        return Redirect($"{_configuration.GetSection("Application:AppDomain").Value}/Success");
    }

    // Route -> OTP Verification
    [HttpPost("otbVerification")]
    public async Task<IActionResult> OtbVerification([FromBody] OTPVerificationDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.OtbVerificationAsync(dto);
        if (!string.IsNullOrEmpty(result))
            return BadRequest(result);

        return Ok(new { message = "Verified successfully" });
    }

    // Route -> Reset Password
    [HttpPost("resetPassword")]
    public async Task<IActionResult> ResetPasswordAsync([FromForm] ResetPasswordDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.ResetPasswordAsync(dto);
        if (!string.IsNullOrEmpty(result.Message))
            return dto.Token.IsNullOrEmpty() ? BadRequest(result.Message) :
                Redirect($"{_configuration.GetSection("Application:AppDomain").Value}/BadRequest");

        return dto.Token.IsNullOrEmpty() ? Ok(new { message = "Password Reset Successfully" }) :
                Redirect($"{_configuration.GetSection("Application:AppDomain").Value}/Success");
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

    // Route -> Refresh Token
    [HttpPost("refreshToken")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
    {
        var refreshToken = dto.RefreshToken ?? Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(refreshToken))
            return BadRequest("Token is required!");

        var result = await _authService.RefreshTokenAsync(refreshToken);

        if (!result.IsAuthenticated)
            return BadRequest(result);

        SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);

        return Ok(result);
    }

    // Route -> Revoke Token
    [HttpPost("revokeToken")]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenDto dto)
    {
        var token = dto.Token ?? Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(token))
            return BadRequest("Token is required!");

        var result = await _authService.RevokeTokenAsync(dto);

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