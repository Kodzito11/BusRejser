using Stripe.Checkout;

namespace BusRejser.Services
{
	public class StripeCheckoutSessionClient : IStripeCheckoutSessionClient
	{
		private readonly SessionService _sessionService = new();

		public Session Create(SessionCreateOptions options)
		{
			return _sessionService.Create(options);
		}

		public Session Get(string sessionId)
		{
			return _sessionService.Get(sessionId);
		}
	}
}
