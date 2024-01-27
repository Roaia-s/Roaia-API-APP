using Microsoft.AspNetCore.Identity;

namespace Roaia.Services;

public class AccountService(UserManager<ApplicationUser> userManager) : IAccountService
{
	private readonly UserManager<ApplicationUser> _userManager = userManager;
	public async Task<UserInfoDto> GetUserInformationAsync(string userId)
	{
		var userInfoDto = new UserInfoDto();
		var user = await _userManager.FindByIdAsync(userId);
		if (user == null)
		{
			return null;
		}
		userInfoDto.UserName = user.UserName;
		userInfoDto.Email = user.Email;
		userInfoDto.PhoneNumber = user.PhoneNumber;
		userInfoDto.FirstName = user.FirstName;
		userInfoDto.LastName = user.LastName;

		return userInfoDto;
	}
}
