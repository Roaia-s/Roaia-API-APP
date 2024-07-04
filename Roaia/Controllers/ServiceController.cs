using OpenAI.Chat;

namespace Roaia.Controllers;
[Route("api/[controller]")]
[ApiController]
public class ServiceController(IAudioService audioService, IOptions<OpenAISettings> openAISettings, IHttpClientFactory httpClientFactory) : ControllerBase
{
    private readonly IAudioService _audioService = audioService;
    private readonly OpenAISettings _openAISettings = openAISettings.Value;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    [HttpPost("process-audio")]
    public async Task<IActionResult> ProcessAudio([FromForm] OpenAIDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var text = await _audioService.ProcessAudioAsync(dto.AudioFile);
        if (string.IsNullOrEmpty(text))
            return BadRequest(_audioService.ErrorMessage);

        return Ok(text);
    }

    [HttpGet("start-chat")]
    public IActionResult StartChat()
    {
        ChatClient client = new(_openAISettings.Model, _openAISettings.SecretKey);

        var completion = client.CompleteChat("Say 'this is a test.'");

        return Ok($"{completion.Value}");
    }

    [HttpPost("ask-chat")]
    public IActionResult StartChat(string question)
    {
        ChatClient client = new(_openAISettings.Model, _openAISettings.SecretKey);

        var completion = client.CompleteChat(question);

        return Ok($"{completion.Value}");
    }

    [HttpPost("process-image")]
    public async Task<IActionResult> ProcessImage(IFormFile image)
    {
        if (image == null || image.Length == 0)
            return BadRequest("Invalid image file.");

        var client = _httpClientFactory.CreateClient();

        using var content = new MultipartFormDataContent();
        using var fileStream = image.OpenReadStream();
        content.Add(new StreamContent(fileStream), "file", image.FileName);

        var response = await client.PostAsync(_openAISettings.ModelUrl, content);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            return Ok(result);
        }

        return StatusCode((int)response.StatusCode, response.ReasonPhrase);
    }
}
