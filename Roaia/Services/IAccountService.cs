namespace Roaia.Services;

public interface IAccountService
{
    Task<UserInfoDto> GetUserInformationAsync(string userId);
    Task<BlindInfoDto> GetBlindInformationAsync(string blindId);
    Task<string> GenerateGlassesIdAsync();
    Task<BlindInfoDto> ModifyBlindInfoAsync(BlindInfoDto dto);
    Task<List<Contact>?> GetAllContactsByIdAsync(string blindId);
    Task<ContactDto> ModifyContactInfoAsync(ContactDto dto);
    Task<ContactDto> AddContactAsync(ContactDto dto);
    Task<List<ContactImagesDto>?> GetAllImagesByGlassesIdAsync(string blindId);
    // delete account by id
    Task<string> DeleteAccountAsync(string userId);

}
