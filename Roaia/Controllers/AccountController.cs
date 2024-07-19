namespace Roaia.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class AccountController(IAccountService accountService, INotificationService notificationService) : Controller
{
    private readonly IAccountService _accountService = accountService;
    private readonly INotificationService _notificationService = notificationService;


    [HttpGet("userinfo/{email}")]
    public async Task<IActionResult> GetUserInfoAsync(string email)
    {
        var result = await _accountService.GetUserInformationAsync(email);
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

    [HttpPost("GenerateBlindId")]
    public async Task<IActionResult> GenerateBlindIdAsync()
    {
        var result = await _accountService.GenerateGlassesIdAsync();
        if (result is null)
            return NotFound(new { message = "Error in Generating Id" });

        return Ok(result);
    }

    [HttpPut("ModifyBlindInfo")]
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

    //Delete contact by id
    [HttpPost("DeleteContact/{contactId}")]
    public async Task<IActionResult> DeleteContactAsync(int contactId)
    {
        var result = await _accountService.DeleteContactAsync(contactId);
        if (!result.IsNullOrEmpty())
            return NotFound(new { message = result });

        return Ok(new { message = "Contact Deleted Successfully" });
    }

    [HttpGet("getAllContacts/{blindId}")]
    public async Task<IActionResult> GetAllContactsByIdAsync(string blindId)
    {
        var result = await _accountService.GetAllContactsByIdAsync(blindId);
        if (result is null)
            return NotFound(new { message = "No Contacts Found or error in blind id" });

        return Ok(result);
    }

    [HttpPost("DeleteAccount/{userId}")]
    public async Task<IActionResult> DeleteAccountAsync(string userId)
    {
        var result = await _accountService.DeleteAccountAsync(userId);
        if (!result.IsNullOrEmpty())
            return NotFound(new { message = $"Error in Deleting Account: {result}" });

        return Ok(new { message = "Account Deleted Successfully" });
    }

    [HttpGet("GetNotificationsByGlassesId/{glassesId}")]
    public async Task<IActionResult> GetNotificationsByGlassesIdAsync(string glassesId)
    {
        var result = await _notificationService.GetNotificationsByGlassesIdAsync(glassesId);

        if (result is null)
            return NotFound(new { message = "glassesId is not found" });

        return Ok(result);
    }

    //delete all notifications for a specific glasses
    [HttpPost("DeleteNotifications/{glassesId}")]
    public async Task<IActionResult> DeleteNotificationsAsync(string glassesId)
    {
        var result = await _notificationService.DeleteNotificationsAsync(glassesId);
        if (result.Message is not null)
            return NotFound(new { message = result.Message });

        return Ok(new { message = "Notifications Deleted Successfully" });
    }

    //delete notification by id
    [HttpPost("DeleteNotificationById/{notificationId}")]
    public async Task<IActionResult> DeleteNotificationAsync(int notificationId)
    {
        var result = await _notificationService.DeleteNotificationAsync(notificationId);
        if (result.Message is not null)
            return NotFound(new { message = result.Message });

        return Ok(new { message = "Notification Deleted Successfully" });
    }

    [HttpPost("ToggleNotificationReadStatus/{notificationId}")]
    public async Task<IActionResult> ToggleNotificationReadStatusAsync(int notificationId)
    {
        var result = await _notificationService.ToggleNotificationReadStatusAsync(notificationId);
        if (!result.IsNullOrEmpty())
            return NotFound(new { message = result });

        return Ok(new { message = "Notification status changed successfully" });
    }

    [HttpPost("MarkAllNotificationsRead/{glassesId}")]
    public async Task<IActionResult> MarkAllNotificationsReadAsync(string glassesId)
    {
        var result = await _notificationService.MarkAllNotificationsReadAsync(glassesId);
        if (!result.IsNullOrEmpty())
            return NotFound(new { message = result });

        return Ok(new { message = "Notifications status changed successfully" });
    }
}
