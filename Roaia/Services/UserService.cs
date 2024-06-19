
using AutoMapper;
namespace Roaia.Services;

public class UserService(UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IMapper mapper) : IUserService

{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<UserDto>> GetAll()
    {
        var users = await _userManager.Users.ToListAsync();
        var dto = _mapper.Map<IEnumerable<UserDto>>(users);

        return dto;
    }

    public async Task<ApplicationUser> GetById(string id)
    {
        return await _userManager.FindByIdAsync(id);
    }

}
