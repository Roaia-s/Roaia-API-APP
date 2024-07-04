namespace Roaia.Core.Models.Dtos;

public class OpenAIDto
{

    // validate audio file with max size 10MB and allowed extensions audios only
    [AllowedExtensions(FileSettings.AllowedAudioExtensions, ErrorMessage = Errors.InvalidAudioUrl),
        MaxFileSize(FileSettings.MaxFileSizeInBytes, ErrorMessage = Errors.MaxSize)]
    public IFormFile? AudioFile { get; set; }

    public string? Question { get; set; }
}
