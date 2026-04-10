using Stripe.Checkout;

namespace BusRejser.Services
{
	public interface IStripeCheckoutSessionClient
	{
		Session Create(SessionCreateOptions options);
		Session Get(string sessionId);
	}
}
