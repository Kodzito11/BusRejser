using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

public class EmailService
{
	private readonly IConfiguration _config;

	public EmailService(IConfiguration config)
	{
		_config = config;
	}

	public async Task SendAsync(string to, string subject, string body)
	{
		var message = new MimeMessage();
		message.From.Add(MailboxAddress.Parse(_config["Email:From"]));
		message.To.Add(MailboxAddress.Parse(to));
		message.Subject = subject;
		message.Body = new TextPart("plain")
		{
			Text = body
		};

		using var client = new SmtpClient();

		await client.ConnectAsync(
			_config["Email:Host"],
			int.Parse(_config["Email:Port"]),
			SecureSocketOptions.StartTls
		);

		await client.AuthenticateAsync(
			_config["Email:Username"],
			_config["Email:Password"]
		);

		await client.SendAsync(message);
		await client.DisconnectAsync(true);
	}
}