using BusRejser.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

public class EmailService
{
	private readonly EmailOptions _emailOptions;

	public EmailService(IOptions<EmailOptions> emailOptions)
	{
		_emailOptions = emailOptions.Value;
	}

	public async Task SendAsync(string to, string subject, string body)
	{
		var message = new MimeMessage();
		message.From.Add(MailboxAddress.Parse(_emailOptions.From));
		message.To.Add(MailboxAddress.Parse(to));
		message.Subject = subject;
		message.Body = new TextPart("plain")
		{
			Text = body
		};

		using var client = new SmtpClient();

		await client.ConnectAsync(
			_emailOptions.Host,
			_emailOptions.Port,
			SecureSocketOptions.StartTls
		);

		await client.AuthenticateAsync(
			_emailOptions.Username,
			_emailOptions.Password
		);

		await client.SendAsync(message);
		await client.DisconnectAsync(true);
	}
}
