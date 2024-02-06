namespace Roaia.Services
{
	public class EmailBodyBuilder : IEmailBodyBuilder
	{
		private readonly IWebHostEnvironment _webHostEnvironment;

		public EmailBodyBuilder(IWebHostEnvironment webHostEnvironment)
		{
			_webHostEnvironment = webHostEnvironment;
		}

		public string GetEmailBody(string imageUrl, string header, string body)
		{
			var filePath = $"{_webHostEnvironment.WebRootPath}/templates/email.html";
			StreamReader str = new(filePath);

			var template = str.ReadToEnd();
			str.Close();

			return template
				.Replace("[imageUrl]", imageUrl)
				.Replace("[header]", header)
				.Replace("[body]", body);
		}
	}
}