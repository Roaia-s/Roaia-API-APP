namespace Roaia.Services;

public interface IAuthService
{
    Task<ApplicationUser> FindByEmail(string email);
    Task<AuthDto> RegisterAsync(RegisterDto dto);
    Task<AuthDto> GetTokenAsync(TokenRequestDto dto);
    Task<string> AddRoleAsync(AddRoleDto dto);
    Task<AuthDto> RefreshTokenAsync(string token);
    Task<bool> RevokeTokenAsync(string token);
    Task<AuthDto> ModfiyUserAsync(ModifyUserDto dto);
    Task<AuthDto> ChangePasswordAsync(ChangePasswordDto dto);
    Task<string> SendResetPasswordEmailAsync(string email);
    Task<string> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
	Task<ApplicationUser> GetUserById(string id); 
    Task<List<UserInfoDto>> GetUsersInfoAsync(); 
    Task<UserInfoDto> GetUserByIdAsync(string userId);
}
