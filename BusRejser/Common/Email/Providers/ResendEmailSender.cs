using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BusRejser.Options;
using Microsoft.Extensions.Options;

namespace BusRejser.Common.Email.Providers
{
	public class ResendEmailSender : IEmailSender
	{
		private readonly HttpClient _httpClient;
		private readonly EmailOptions _emailOptions;

		public ResendEmailSender(
			HttpClient httpClient,
			IOptions<EmailOptions> emailOptions)
		{
			_httpClient = httpClient;
			_emailOptions = emailOptions.Value;
		}

		public async Task SendAsync(
			EmailMessage message,
			CancellationToken cancellationToken = default)
		{
			var payload = new
			{
				from = _emailOptions.From,
				to = new[] { message.To },
				subject = message.Subject,
				html = message.HtmlBody,
				text = message.TextBody
			};

			using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails");

			request.Headers.Authorization =
				new AuthenticationHeaderValue("Bearer", _emailOptions.ApiKey);

			request.Content = new StringContent(
				JsonSerializer.Serialize(payload),
				Encoding.UTF8,
				"application/json");

			using var response = await _httpClient.SendAsync(request, cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

				throw new InvalidOperationException(
					$"Resend email failed with status {(int)response.StatusCode}: {responseBody}");
			}
		}
	}
}