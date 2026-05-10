using Stripe.Checkout;

namespace BusRejser.Features.Payments.Services.Interfaces
{
	public interface IStripeCheckoutSessionClient
	{
		Session Create(SessionCreateOptions options);
		Session Get(string sessionId);
	}
}
