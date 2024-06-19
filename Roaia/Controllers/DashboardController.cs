using AutoMapper;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Roaia.Controllers;
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = AppRoles.Admin)]
public class DashboardController(UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IMapper mapper,
    IUserService userService, IAuthService authService) : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;
    private readonly IMapper _mapper = mapper;
    private readonly IUserService _userService = userService;
    private readonly IAuthService _authService = authService;



    // send mail to multiple users
    [HttpPost("sendMailNews")]
    public async Task<IActionResult> SendMailNewsAsync([FromForm] MailNewsDto dto)
    {
        var result = await _authService.SendMailNewsAsync(dto);
        if (!result.IsNullOrEmpty())
            return BadRequest(result);

        return Ok(new { message = "Email Send Successfully" });
    }



    [HttpGet("getAllUsers")]
    public async Task<IActionResult> GetAllAsync()
    {
        var dto = await _userService.GetAll();
        return Ok(dto);
    }

    [HttpGet("createUser")]
    public async Task<IActionResult> Create()
    {
        UserFormDto dto = new()
        {
            Roles = await _roleManager.Roles
            .Select(r => new SelectListItem { Value = r.Name, Text = r.Name })
            .ToListAsync()
        };
        return Ok(dto);
    }

    [HttpPost("createUser")]
    public async Task<IActionResult> CreateUserAsync(UserFormDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        ApplicationUser user = new()
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            UserName = dto.UserName,
            Email = dto.Email,
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRolesAsync(user, dto.SelectedRoles);

            var Model = _mapper.Map<UserDto>(user);
            return Ok(Model);
        }

        return BadRequest(string.Join(',', result.Errors.Select(e => e.Description)));
    }

    [HttpDelete]
    [Route("toggleStatus/{id}")]
    public async Task<IActionResult> ToggleStatus(string id)
    {
        var user = await _userService.GetById(id);
        if (user is null)
            return NotFound($"No user was found with ID: {id}");

        user.IsDeleted = !user.IsDeleted;
        user.LastUpdatedOn = DateTime.Now;

        await _userManager.UpdateAsync(user);

        await _userManager.UpdateSecurityStampAsync(user);

        return Ok(user.LastUpdatedOn?.ToString("MMM dd, yyyy"));
    }

    [HttpPost("resetPassword/{id}")]
    public async Task<IActionResult> ResetPassword(string id, ResetPasswordDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userService.GetById(id);

        if (user is null)
            return NotFound($"No user was found with ID: {id}");

        var currentPasswordHash = user.PasswordHash;
        await _userManager.RemovePasswordAsync(user);
        var result = await _userManager.AddPasswordAsync(user, dto.NewPassword);

        if (result.Succeeded)
        {
            user.LastUpdatedOn = DateTime.Now;

            await _userManager.UpdateAsync(user);

            var Model = _mapper.Map<UserDto>(user);
            return Ok(Model);
        }

        user.PasswordHash = currentPasswordHash;
        await _userManager.UpdateAsync(user);

        return BadRequest(string.Join(',', result.Errors.Select(e => e.Description)));
    }

    [HttpPost]
    [Route("unlock/{id}")]
    public async Task<IActionResult> Unlock(string id)
    {
        var user = await _userService.GetById(id);
        if (user is null)
            return NotFound($"No user was found with ID: {id}");

        var isLocked = await _userManager.IsLockedOutAsync(user);

        if (isLocked)
            await _userManager.SetLockoutEndDateAsync(user, null);

        return Ok();
    }

    [HttpPut]
    [Route("Edit/{id}")]
    public async Task<IActionResult> Edit(string id, UserFormDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userService.GetById(id);

        if (user is null)
            return NotFound($"No user was found with ID: {id}");

        user = _mapper.Map(dto, user);
        user.LastUpdatedOn = DateTime.Now;

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            var updatedRoles = !currentRoles.SequenceEqual(dto.SelectedRoles);

            if (updatedRoles)
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRolesAsync(user, dto.SelectedRoles);
            }

            await _userManager.UpdateSecurityStampAsync(user);

            var Model = _mapper.Map<UserDto>(user);
            return Ok(Model);
        }

        return BadRequest(string.Join(',', result.Errors.Select(r => r.Description)));
    }
}
