
namespace Roaia.Services;

public class ImageService(IWebHostEnvironment webHostEnvironment) : IImageService
{
    private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;

    private List<string> _allowedExtensions = new() { ".jpg", ".jpeg", ".png" };
    private long _maxAllowedSize = 4194304;

    public async Task<(bool isUploaded, string? errorMessage)> UploadAsync(IFormFile image, string imageName, string folderPath, bool hasThumbnail)
    {
        var extension = Path.GetExtension(image.FileName).ToLower();

        if (!_allowedExtensions.Contains(extension))
            return (isUploaded: false, errorMessage: Errors.NotAllowedExtensions);

        if (image.Length > _maxAllowedSize)
            return (isUploaded: false, errorMessage: Errors.MaxSize);


        var path = Path.Combine($"{_webHostEnvironment.WebRootPath}{folderPath}", imageName);

        using var stream = File.Create(path);
        await image.CopyToAsync(stream);
        stream.Dispose();

        if (hasThumbnail)
        {
            var thumbPath = Path.Combine($"{_webHostEnvironment.WebRootPath}{folderPath}/thumb", imageName);

            using var LoadedImage = Image.Load(image.OpenReadStream());
            var ratio = (float)LoadedImage.Width / 200;
            var height = LoadedImage.Height / ratio;
            LoadedImage.Mutate(i => i.Resize(width: 200, height: (int)height));
            LoadedImage.Save(thumbPath);
        }

        return (isUploaded: true, errorMessage: null);
    }

    public void Delete(string imagePath, string? imageThumbnailPath)
    {
        var oldImagePath = $"{_webHostEnvironment.WebRootPath}{imagePath}";

        if (File.Exists(oldImagePath) && !imagePath.Equals("/images/avatar.png"))
            File.Delete(oldImagePath);

        if (!string.IsNullOrEmpty(imageThumbnailPath))
        {
            var oldThumbPath = $"{_webHostEnvironment.WebRootPath}{imageThumbnailPath}";

            if (File.Exists(oldThumbPath))
                File.Delete(oldThumbPath);
        }
    }
}
