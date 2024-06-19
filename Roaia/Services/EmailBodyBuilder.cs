namespace Roaia.Services
{
    public class EmailBodyBuilder(IWebHostEnvironment webHostEnvironment, IConfiguration configuration) : IEmailBodyBuilder
    {
        private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;
        private readonly IConfiguration _configuration = configuration;


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
                .Replace("[feedback]", _configuration.GetSection("Application:Feedback").Value)
                .Replace("[twitter]", _configuration.GetSection("Application:Twitter").Value)
                .Replace("[linkedin]", _configuration.GetSection("Application:Linkedin").Value)
                .Replace("[facebook]", _configuration.GetSection("Application:Facebook").Value)
                .Replace("[instagram]", _configuration.GetSection("Application:Instagram").Value);
        }
    }
}