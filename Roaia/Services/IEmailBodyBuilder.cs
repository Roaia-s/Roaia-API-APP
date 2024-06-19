namespace Roaia.Services
{
    public interface IEmailBodyBuilder
    {
        string GetEmailBody(string fileName);
        string GetEmailBody(string imageUrl, string header, string body, string? url, string warning, string linkTitle);
    }
}