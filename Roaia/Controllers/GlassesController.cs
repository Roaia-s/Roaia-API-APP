﻿namespace Roaia.Controllers;
[Route("api/[controller]")]
[ApiController]
public class GlassesController(IAccountService accountService, INotificationService notificationService, IHubContext<GPSHub> hubContext) : ControllerBase
{
    private readonly IAccountService _accountService = accountService;
    private readonly INotificationService _notificationService = notificationService;
    private readonly IHubContext<GPSHub> _hubContext = hubContext;

    [HttpPut("ModifyContactInfo")]
    public async Task<IActionResult> ModifyContactInfo([FromForm] ContactDto dto)
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
    public async Task<IActionResult> AddContact([FromForm] ContactDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _accountService.AddContactAsync(dto);
        if (result.Message is not null)
            return NotFound(new { message = result.Message });

        result.Message = "Contact Added Successfully";
        return Ok(result);
    }

    [HttpGet("GetAllImagesByGlassesId/{blindId}")]
    public async Task<IActionResult> GetAllImagesByGlassesId(string blindId)
    {
        var result = await _accountService.GetAllImagesByGlassesIdAsync(blindId);
        if (result is null)
            return NotFound(new { message = "No Contacts Found" });

        return Ok(result);
    }

    [HttpGet("getAllContacts/{blindId}")]
    public async Task<IActionResult> GetAllContactsByIdAsync(string blindId)
    {
        var result = await _accountService.GetAllContactsByIdAsync(blindId);
        if (result is null)
            return NotFound(new { message = "No Contacts Found or error in blind id" });

        return Ok(result);
    }

    [HttpPost("SendNotification")]
    public async Task<IActionResult> SendMessageAsync([FromBody] NotificationDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _notificationService.SendNotificationAsync(request);

        if (result.Message is not null)
            return BadRequest(new { message = result.Message });

        result.Message = "Notification sent successfully!";
        return Ok(result);
    }

    // send gps data to users with the same glasses id
    [HttpPost("SendGPSData")]
    public async Task<IActionResult> SendGPSData([FromBody] GpsDto gpsData)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _hubContext.Clients.Group(gpsData.GlassesId).SendAsync("ReceiveGpsData", gpsData);

        return Ok("sent successfully!");
    }

    // add notification to database
    [HttpPost("ManualNotification")]
    public async Task<IActionResult> ManualNotification([FromBody] NotificationDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _accountService.ManualNotificationAsync(request);

        if (result.Message is not null)
            return BadRequest(new { message = result.Message });

        result.Message = "Notification added successfully!";
        return Ok(result);
    }
}
