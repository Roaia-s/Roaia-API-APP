namespace Roaia.Services;

public interface IAccountService
{
	Task<UserInfoDto> GetUserInformationAsync(string userId);
}
