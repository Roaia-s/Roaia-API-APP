using OpenAI.Chat;

namespace Roaia.Controllers;
[Route("api/[controller]")]
[ApiController]
public class ServiceController(IAudioService audioService, IConfiguration configuration) : ControllerBase
{
    private readonly IAudioService _audioService = audioService;
    private readonly IConfiguration _configuration = configuration;

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
        ChatClient client = new("gpt-4o", $"{_configuration["OpenAI:SecretKey"]}");

        var completion = client.CompleteChat("Say 'this is a test.'");

        return Ok($"{completion.Value}");
    }

    [HttpPost("ask-chat")]
    public IActionResult StartChat(string question)
    {
        ChatClient client = new("gpt-4o", $"{_configuration["OpenAI:SecretKey"]}");

        var completion = client.CompleteChat(question);

        return Ok($"{completion.Value}");
    }
}
