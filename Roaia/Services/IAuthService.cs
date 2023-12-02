namespace Roaia.Services;

public interface IAuthService
{
    Task<Auth> RegisterAsync(Register model);
}
