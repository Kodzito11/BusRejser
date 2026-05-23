namespace BusRejser.Common.Email
{
	public interface IEmailSender
	{
		Task SendAsync(
			EmailMessage message,
			CancellationToken cancellationToken = default);
	}
}