namespace Roaia.Services.OpenAI;

public interface IAudioService
{
    Task<string> TranscribeAudioAsync(byte[] audioData);
    Task<string> ProcessAudioAsync(IFormFile audioFile);
    // error message
    string ErrorMessage { get; }
}
