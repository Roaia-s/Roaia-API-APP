namespace Roaia.Services;

public interface IAuthService
{
    Task<AuthDto> RegisterAsync(RegisterDto dto);
    Task<AuthDto> GetTokenAsync(TokenRequestDto dto);
    Task<string> AddRoleAsync(AddRoleDto dto);
    Task<AuthDto> RefreshTokenAsync(string token);
    Task<bool> RevokeTokenAsync(RevokeTokenDto dto);
    Task<AuthDto> ModfiyUserAsync(ModifyUserDto dto);
    Task<AuthDto> ChangePasswordAsync(ChangePasswordDto dto);
    Task<string> VerifyEmailAsync(string email, string token);
    Task<string> SendOTPCodeAsync(string email);
    //send mail news to multiple users
    Task<string> SendMailNewsAsync(MailNewsDto dto);
    Task<AuthDto> ResetPasswordAsync(ResetPasswordDto dto);
    Task<string> OtbVerificationAsync(OTPVerificationDto dto);
    Task<List<UserInfoDto>?> GetUsersInfoAsync();
    Task<UserInfoDto> GetUserByIdAsync(string userId);
    // unsubscribe from mail news
    Task<string> UnsubscribeMailNewsAsync(string email);
}
