namespace Roaia.Attributes;

public class ValidUrlAttribute : ValidationAttribute
{

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var url = value as string;

        if (!string.IsNullOrWhiteSpace(url))
        {

            if (Uri.TryCreate(url.ToString(), UriKind.Absolute, out var uriResult) &&
                (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            {
                // Additional checks for file extensions
                var fileExtension = System.IO.Path.GetExtension(uriResult.AbsolutePath).ToLowerInvariant();
                if (!(IsValidImageFile(fileExtension) || IsValidAudioFile(fileExtension) || IsValidVideoFile(fileExtension)))
                    return new ValidationResult(ErrorMessage ?? "The URL is not valid.");
            }
        }

        return ValidationResult.Success;
    }

    private bool IsValidImageFile(string extension)
    {
        return extension == ".jpg" || extension == ".jpeg" || extension == ".png";
    }

    private bool IsValidAudioFile(string extension)
    {
        return extension == ".mp3" || extension == ".wav";
    }

    private bool IsValidVideoFile(string extension)
    {
        return extension == ".mp4";
    }

}
