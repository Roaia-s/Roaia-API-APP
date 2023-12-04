namespace Roaia.Services;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAll();
    Task<ApplicationUser> GetById(string id);
}
