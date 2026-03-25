using BusRejser.DTOs;
using BusRejser.Exceptions;
using BusRejserLibrary.Repositories;
using BusRejserLibrary.Services;
using Stripe;
using Stripe.Checkout;

namespace BusRejser.Services
{
	public class StripeService
	{
		private readonly RejseRepository _rejseRepository;
		private readonly BookingService _bookingService;
		private readonly IConfiguration _configuration;

		public StripeService(
			RejseRepository rejseRepository,
			BookingService bookingService,
			IConfiguration configuration)
		{
			_rejseRepository = rejseRepository;
			_bookingService = bookingService;
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
				throw new ValidationException("Antal pladser skal være mindst 1.");

			var rejse = _rejseRepository.GetById(request.RejseId);
			if (rejse == null)
				throw new NotFoundException("Rejse findes ikke.");

			var availableSeats = rejse.MaxSeats - rejse.BookedSeats;
			if (request.AntalPladser > availableSeats)
				throw new ValidationException("Ikke nok ledige pladser.");

			if (rejse.Price < 0)
				throw new ValidationException("Ugyldig pris på rejse.");

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

			var sessionService = new SessionService();
			var session = sessionService.Create(options);

			if (string.IsNullOrWhiteSpace(session.Url))
				throw new Exception("Stripe returnerede ikke en checkout-url.");

			return session.Url;
		}

		public void HandleWebhook(string json, string stripeSignature)
		{
			var webhookSecret = _configuration["Stripe:WebhookSecret"];
			if (string.IsNullOrWhiteSpace(webhookSecret))
				throw new Exception("Stripe webhook secret mangler.");

			var stripeEvent = EventUtility.ConstructEvent(
				json,
				stripeSignature,
				webhookSecret
			);

			if (stripeEvent.Type != EventTypes.CheckoutSessionCompleted)
				return;

			var session = stripeEvent.Data.Object as Session;
			if (session == null)
				throw new ValidationException("Stripe session mangler.");

			HandleCheckoutSessionCompleted(session);
		}

		private void HandleCheckoutSessionCompleted(Session session)
		{
			var request = BuildWebhookBookingRequest(session);
			_bookingService.CreateFromStripe(request);
		}

		private StripeWebhookBookingRequest BuildWebhookBookingRequest(Session session)
		{
			if (string.IsNullOrWhiteSpace(session.Id))
				throw new ValidationException("Stripe session id mangler.");

			var metadata = session.Metadata;
			if (metadata == null)
				throw new ValidationException("Stripe metadata mangler.");

			if (!metadata.TryGetValue("rejseId", out var rejseIdRaw) || !int.TryParse(rejseIdRaw, out var rejseId))
				throw new ValidationException("Ugyldig rejseId.");

			if (!metadata.TryGetValue("antalPladser", out var antalPladserRaw) || !int.TryParse(antalPladserRaw, out var antalPladser))
				throw new ValidationException("Ugyldig antalPladser.");

			if (!metadata.TryGetValue("kundeNavn", out var kundeNavn) || string.IsNullOrWhiteSpace(kundeNavn))
				throw new ValidationException("Manglende kundeNavn.");

			if (!metadata.TryGetValue("kundeEmail", out var kundeEmail) || string.IsNullOrWhiteSpace(kundeEmail))
				throw new ValidationException("Manglende kundeEmail.");

			int? userId = null;
			if (metadata.TryGetValue("userId", out var userIdRaw) &&
				!string.IsNullOrWhiteSpace(userIdRaw) &&
				int.TryParse(userIdRaw, out var parsedUserId))
			{
				userId = parsedUserId;
			}

			return new StripeWebhookBookingRequest
			{
				RejseId = rejseId,
				AntalPladser = antalPladser,
				KundeNavn = kundeNavn,
				KundeEmail = kundeEmail,
				UserId = userId,
				StripeSessionId = session.Id,
				StripePaymentIntentId = session.PaymentIntentId,
				TotalPrice = (session.AmountTotal ?? 0) / 100m
			};
		}
	}
}