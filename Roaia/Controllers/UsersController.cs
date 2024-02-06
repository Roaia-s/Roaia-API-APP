using Microsoft.AspNetCore.Mvc;

namespace Roaia.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class UsersController(IAccountService accountService) : Controller
{
	private readonly IAccountService _accountService = accountService;

	[HttpGet("userinfo/{userId}")]
	public async Task<IActionResult> GetUserInfoAsync(string userId)
	{
		var result = await _accountService.GetUserInformationAsync(userId);
		if (result.Message is not null)
			return NotFound(new { message = result.Message });

		return Ok(result);
	}
}
