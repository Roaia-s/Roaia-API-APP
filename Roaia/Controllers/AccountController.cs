using Microsoft.AspNetCore.Mvc;

namespace Roaia.Controllers;

//[Authorize]
[Route("api/[controller]")]
[ApiController]
public class AccountController(IAccountService accountService) : Controller
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
	
	[HttpGet("blindInfo/{blindId}")]
	public async Task<IActionResult> GetBlindInfoAsync(string blindId)
	{
		var result = await _accountService.GetBlindInformationAsync(blindId);
		if (result.Message is not null)
			return NotFound(new { message = result.Message });

		return Ok(result);
	}

	[HttpPut ("ModifyBlindInfo")]
	public async Task<IActionResult> BlindInfoAsync([FromForm] BlindInfoDto dto)
	{
		if (!ModelState.IsValid)
			return BadRequest(ModelState);

		var result = await _accountService.ModifyBlindInfoAsync(dto);
		if (result.Message is not null)
			return NotFound(new { message = result.Message });

		result.Message = "Blind Information Modified Successfully";
		return Ok(result);
	}

	[HttpPut("ModifyContactInfo")]
	public async Task<IActionResult> ModifyContactInfoAsync([FromForm] ContactDto dto)
	{
		if (!ModelState.IsValid)
			return BadRequest(ModelState);

		var result = await _accountService.ModifyContactInfoAsync(dto);
		if (result.Message is not null)
			return NotFound(new { message = result.Message });

		result.Message = "Contact Information Modified Successfully";
		return Ok(result);
	}

	[HttpPost("AddContact")]
	public async Task<IActionResult> AddContactAsync([FromForm] ContactDto dto)
	{
		if (!ModelState.IsValid)
			return BadRequest(ModelState);

		var result = await _accountService.AddContactAsync(dto);
		if (result.Message is not null)
			return NotFound(new { message = result.Message });

		result.Message = "Contact Added Successfully";
		return Ok(result);
	}

	[HttpGet("contacts/{blindId}")]
	public async Task<IActionResult> GetAllContactsByIdAsync(string blindId)
	{
		var result = await _accountService.GetAllContactsByIdAsync(blindId);
		if (result is null)
			return NotFound(new { message = "No Contacts Found" });

		return Ok(result);
	}
}
