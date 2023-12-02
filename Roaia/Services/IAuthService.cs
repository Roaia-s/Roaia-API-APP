namespace Roaia.Services;

public interface IAuthService
{
    Task<Auth> RegisterAsync(Register model);
    Task<Auth> GetTokenAsync(TokenRequest model);
    Task<string> AddRoleAsync(AddRole model);
    Task<Auth> RefreshTokenAsync(string token);
    Task<bool> RevokeTokenAsync(string token);
}
