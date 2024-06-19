namespace Roaia.Services.OpenAI;

public class AudioService(IOptions<SpeechSettings> options) : IAudioService
{
    private readonly SpeechSettings _options = options.Value;

    private string errorMessage;
    public string ErrorMessage => !string.IsNullOrEmpty(errorMessage) ? errorMessage : string.Empty;

    public async Task<string> TranscribeAudioAsync(byte[] audioData)
    {
        var speechConfig = SpeechConfig.FromSubscription(_options.SubscriptionKey, _options.Region);
        speechConfig.OutputFormat = OutputFormat.Simple;
        var audioInputStream = AudioInputStream.CreatePushStream();

        audioInputStream.Write(audioData);
        audioInputStream.Close();

        var audioConfig = AudioConfig.FromStreamInput(audioInputStream);
        using var recognizer = new SpeechRecognizer(speechConfig, audioConfig);

        var result = await recognizer.RecognizeOnceAsync();
        if (result.Reason != ResultReason.RecognizedSpeech)
            errorMessage = $"Unable to recognize speech: {result.Reason}";

        return result.Text;
    }

    public async Task<string> ProcessAudioAsync(IFormFile audioFile)
    {
        if (audioFile == null || audioFile.Length == 0)
            return errorMessage = "Invalid audio file.";

        using var memoryStream = new MemoryStream();
        await audioFile.CopyToAsync(memoryStream);
        var audioData = memoryStream.ToArray();

        var text = await TranscribeAudioAsync(audioData);
        return text;

    }
}