namespace Roaia.Services;

public class AccountService(UserManager<ApplicationUser> userManager) : IAccountService
{
	private readonly UserManager<ApplicationUser> _userManager = userManager;
	public async Task<UserInfoDto> GetUserInformationAsync(string userId)
	{
		var user = await _userManager.FindByIdAsync(userId);
		if (user is null)
			return new UserInfoDto { Message = "This User Does Not  Exist" };

		UserInfoDto userInfoDto = new();

		userInfoDto.UserName = user.UserName;
		userInfoDto.Email = user.Email;
		userInfoDto.PhoneNumber = user.PhoneNumber;
		userInfoDto.FirstName = user.FirstName;
		userInfoDto.LastName = user.LastName;
		userInfoDto.ImageUrl = user.ImageUrl;

		return userInfoDto;
	}
}
