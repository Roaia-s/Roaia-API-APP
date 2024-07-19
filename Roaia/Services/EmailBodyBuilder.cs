namespace Roaia.Services
{
    public class EmailBodyBuilder(IWebHostEnvironment webHostEnvironment, IOptions<ApplicationSettings> applicationSettings) : IEmailBodyBuilder
    {
        private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;
        private readonly IOptions<ApplicationSettings> _applicationSettings = applicationSettings;


        public string GetEmailBody(string fileName)
        {
            var filePath = $"{_webHostEnvironment.WebRootPath}/templates/{fileName}";
            StreamReader str = new(filePath);

            var template = str.ReadToEnd();
            str.Close();

            return template;
        }

        public string GetEmailBody(string imageUrl, string header, string body, string? url, string warning, string linkTitle)
        {
            var filePath = $"{_webHostEnvironment.WebRootPath}/templates/email.html";
            StreamReader str = new(filePath);

            var template = str.ReadToEnd();
            str.Close();

            return template
                .Replace("[imageUrl]", imageUrl)
                .Replace("[header]", header)
                .Replace("[body]", body)
                .Replace("[warning]", warning)
                .Replace("[url]", url)
                .Replace("[linkTitle]", linkTitle)
                .Replace("[feedback]", _applicationSettings.Value.Feedback)
                .Replace("[twitter]", _applicationSettings.Value.Twitter)
                .Replace("[linkedin]", _applicationSettings.Value.Linkedin)
                .Replace("[facebook]", _applicationSettings.Value.Facebook)
                .Replace("[instagram]", _applicationSettings.Value.Instagram)
                .Replace("[appName]", _applicationSettings.Value.AppName)
                .Replace("[year]", DateTime.Now.Year.ToString());
        }
    }
}