namespace Roaia.Services;

public interface IAuthService
{
    Task<AuthDto> RegisterAsync(RegisterDto dto);
    Task<AuthDto> GetTokenAsync(TokenRequestDto dto);
    Task<string> AddRoleAsync(AddRoleDto dto);
    Task<AuthDto> RefreshTokenAsync(string token);
    Task<bool> RevokeTokenAsync(string token);
}
