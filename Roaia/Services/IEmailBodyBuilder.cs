﻿namespace Roaia.Services
{
	public interface IEmailBodyBuilder
	{
		string GetEmailBody(string imageUrl, string header, string body);
	}
}