using BusRejser.DTOs;
using BusRejserLibrary.Repositories;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace BusRejser.Services
{
	public class StripeService
	{
		private readonly RejseRepository _rejseRepository;
		private readonly IConfiguration _configuration;

		public StripeService(RejseRepository rejseRepository, IConfiguration configuration)
		{
			_rejseRepository = rejseRepository;
			_configuration = configuration;

			var secretKey = _configuration["Stripe:SecretKey"];
			if (string.IsNullOrWhiteSpace(secretKey))
				throw new Exception("Stripe:SecretKey mangler.");

			StripeConfiguration.ApiKey = secretKey;
		}

		public string CreateCheckoutSession(
			CreateCheckoutSessionRequest request,
			int? userId,
			string origin)
		{
			if (request.AntalPladser <= 0)
				throw new Exception("Antal pladser skal være mindst 1.");

			var rejse = _rejseRepository.GetById(request.RejseId);
			if (rejse == null)
				throw new Exception("Rejse findes ikke.");

			var availableSeats = rejse.MaxSeats - rejse.BookedSeats;
			if (request.AntalPladser > availableSeats)
				throw new Exception("Ikke nok ledige pladser.");

			if (rejse.Price < 0)
				throw new Exception("Ugyldig pris på rejse.");

			var totalPrice = rejse.Price * request.AntalPladser;

			var options = new SessionCreateOptions
			{
				Mode = "payment",
				SuccessUrl = $"{origin}/betaling/success?session_id={{CHECKOUT_SESSION_ID}}",
				CancelUrl = $"{origin}/betaling/cancel",
				CustomerEmail = request.KundeEmail,
				LineItems = new List<SessionLineItemOptions>
				{
					new SessionLineItemOptions
					{
						Quantity = request.AntalPladser,
						PriceData = new SessionLineItemPriceDataOptions
						{
							Currency = "dkk",
							UnitAmount = (long)(rejse.Price * 100),
							ProductData = new SessionLineItemPriceDataProductDataOptions
							{
								Name = rejse.Title,
								Description = $"{rejse.Destination} | {rejse.StartAt:dd-MM-yyyy HH:mm}"
							}
						}
					}
				},
				Metadata = new Dictionary<string, string>
				{
					["rejseId"] = request.RejseId.ToString(),
					["antalPladser"] = request.AntalPladser.ToString(),
					["kundeNavn"] = request.KundeNavn,
					["kundeEmail"] = request.KundeEmail,
					["userId"] = userId?.ToString() ?? ""
				}
			};

			var service = new SessionService();
			var session = service.Create(options);

			return session.Url;
		}
	}
}