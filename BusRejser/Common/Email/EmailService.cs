namespace BusRejser.Common.Email
{
	public class EmailService
	{
		private readonly IEmailSender _emailSender;

		public EmailService(IEmailSender emailSender)
		{
			_emailSender = emailSender;
		}

		public async Task SendPasswordResetAsync(
			string to,
			string resetUrl,
			CancellationToken cancellationToken = default)
		{
			var message = new EmailMessage
			{
				To = to,
				Subject = "Nulstil password",
				TextBody = $"Aabn dette link for at nulstille dit password: {resetUrl}",
				HtmlBody = $"""
                    <p>Aabn dette link for at nulstille dit password:</p>
                    <p><a href="{resetUrl}">Nulstil password</a></p>
                    <p>Hvis du ikke har bedt om dette, kan du ignorere denne mail.</p>
                    """
			};

			await _emailSender.SendAsync(message, cancellationToken);
		}
	}
}