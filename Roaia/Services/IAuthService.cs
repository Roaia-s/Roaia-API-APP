namespace Roaia.Services;

public interface IAuthService
{
    Task<Auth> RegisterAsync(Register model);
    Task<string> AddRoleAsync(AddRole model);
    Task<Auth> GetTokenAsync(TokenRequest model);
}
