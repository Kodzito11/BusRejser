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
		private readonly ILogger<StripeService> _logger;

		public StripeService(
			RejseRepository rejseRepository,
			BookingService bookingService,
			IConfiguration configuration,
			ILogger<StripeService> logger)
		{
			_rejseRepository = rejseRepository;
			_bookingService = bookingService;
			_configuration = configuration;
			_logger = logger;

			var secretKey = _configuration["Stripe:SecretKey"];
			if (string.IsNullOrWhiteSpace(secretKey))
			{
				_logger.LogError("Stripe:SecretKey is missing from configuration");
				throw new ValidationException("Stripe:SecretKey mangler.");
			}

			StripeConfiguration.ApiKey = secretKey;
			_logger.LogInformation("StripeService initialized successfully");
		}

		public string CreateCheckoutSession(
			CreateCheckoutSessionRequest request,
			int? userId,
			string origin)
		{
			if (request == null)
			{
				_logger.LogWarning("CreateCheckoutSession called with null request");
				throw new ValidationException("Request må ikke være null.");
			}

			_logger.LogInformation(
				"Creating Stripe checkout session for RejseId {RejseId}, AntalPladser {AntalPladser}, UserId {UserId}",
				request.RejseId,
				request.AntalPladser,
				userId
			);

			if (request.AntalPladser <= 0)
			{
				_logger.LogWarning(
					"Invalid AntalPladser {AntalPladser} for RejseId {RejseId}",
					request.AntalPladser,
					request.RejseId
				);
				throw new ValidationException("Antal pladser skal være mindst 1.");
			}

			var rejse = _rejseRepository.GetById(request.RejseId);
			if (rejse == null)
			{
				_logger.LogWarning("Rejse not found for RejseId {RejseId}", request.RejseId);
				throw new NotFoundException("Rejse findes ikke.");
			}

			var availableSeats = rejse.MaxSeats - rejse.BookedSeats;
			if (request.AntalPladser > availableSeats)
			{
				_logger.LogWarning(
					"Not enough seats for RejseId {RejseId}. Requested {RequestedSeats}, Available {AvailableSeats}",
					request.RejseId,
					request.AntalPladser,
					availableSeats
				);
				throw new ValidationException("Ikke nok ledige pladser.");
			}

			if (rejse.Price < 0)
			{
				_logger.LogError(
					"Invalid negative price {Price} for RejseId {RejseId}",
					rejse.Price,
					rejse.RejseId
				);
				throw new ValidationException("Ugyldig pris på rejse.");
			}

			if (string.IsNullOrWhiteSpace(origin))
			{
				_logger.LogWarning("Origin was missing when creating Stripe checkout session. Falling back to localhost");
				origin = "http://localhost:5173";
			}

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
			{
				_logger.LogError(
					"Stripe returned no checkout URL for RejseId {RejseId}",
					request.RejseId
				);
				throw new ConflictException("Stripe returnerede ikke en checkout-url.");
			}

			_logger.LogInformation(
				"Stripe checkout session created successfully. SessionId {SessionId}, RejseId {RejseId}",
				session.Id,
				request.RejseId
			);

			return session.Url;
		}

		public void HandleWebhook(string json, string stripeSignature)
		{
			_logger.LogInformation("Handling Stripe webhook");

			var webhookSecret = _configuration["Stripe:WebhookSecret"];

			_logger.LogInformation(
				"Stripe webhook received. SecretPresent {SecretPresent}, SignaturePresent {SignaturePresent}, BodyLength {BodyLength}",
				!string.IsNullOrWhiteSpace(webhookSecret),
				!string.IsNullOrWhiteSpace(stripeSignature),
				json?.Length ?? 0
			);

			if (string.IsNullOrWhiteSpace(webhookSecret))
			{
				_logger.LogError("Stripe webhook secret is missing");
				throw new ValidationException("Stripe webhook secret mangler.");
			}

			var stripeEvent = EventUtility.ConstructEvent(
				json,
				stripeSignature,
				webhookSecret,
				throwOnApiVersionMismatch: false
			);

			_logger.LogInformation(
				"Stripe webhook event constructed successfully. EventType {EventType}",
				stripeEvent.Type
			);

			if (stripeEvent.Type != EventTypes.CheckoutSessionCompleted)
			{
				_logger.LogInformation(
					"Ignoring Stripe event type {EventType}",
					stripeEvent.Type
				);
				return;
			}

			var session = stripeEvent.Data.Object as Session;
			if (session == null)
			{
				_logger.LogError("Stripe checkout.session.completed event did not contain a valid Session object");
				throw new ValidationException("Stripe session mangler.");
			}

			_logger.LogInformation(
				"Processing checkout.session.completed for SessionId {SessionId}",
				session.Id
			);

			HandleCheckoutSessionCompleted(session);
		}

		private void HandleCheckoutSessionCompleted(Session session)
		{
			_logger.LogInformation(
				"Building webhook booking request for Stripe SessionId {SessionId}",
				session.Id
			);

			var request = BuildWebhookBookingRequest(session);

			_logger.LogInformation(
				"Creating booking from Stripe SessionId {SessionId} for RejseId {RejseId}",
				request.StripeSessionId,
				request.RejseId
			);

			_bookingService.CreateFromStripe(request);

			_logger.LogInformation(
				"Booking flow completed for Stripe SessionId {SessionId}",
				request.StripeSessionId
			);
		}

		public CheckoutStatusResponse GetCheckoutStatus(string sessionId)
		{
			if (string.IsNullOrWhiteSpace(sessionId))
			{
				_logger.LogWarning("GetCheckoutStatus called with missing sessionId");
				throw new ValidationException("Session id mangler.");
			}

			_logger.LogInformation(
				"Getting checkout status for Stripe SessionId {SessionId}",
				sessionId
			);

			var sessionService = new SessionService();
			var session = sessionService.Get(sessionId);

			if (session == null)
			{
				_logger.LogWarning(
					"Stripe session not found for SessionId {SessionId}",
					sessionId
				);
				throw new NotFoundException("Stripe session blev ikke fundet.");
			}

			var isPaid = session.PaymentStatus == "paid";
			var booking = _bookingService.GetByStripeSessionId(sessionId);

			if (isPaid && booking == null)
			{
				_logger.LogInformation(
					"Paid Stripe session {SessionId} has no booking yet. Attempting fallback booking creation.",
					sessionId
				);

				try
				{
					var request = BuildWebhookBookingRequest(session);

					_logger.LogInformation(
						"Fallback request built: RejseId {RejseId}, Seats {Seats}, Email {Email}",
						request.RejseId,
						request.AntalPladser,
						request.KundeEmail
					);

					_bookingService.CreateFromStripe(request);

					booking = _bookingService.GetByStripeSessionId(sessionId);

					_logger.LogInformation(
						"Fallback booking creation finished. BookingExists {Exists}",
						booking != null
					);
				}
				catch (Exception ex)
				{
					_logger.LogError(
						ex,
						"Fallback booking creation FAILED for session {SessionId}",
						sessionId
					);

					throw;
				}
			}

			var result = new CheckoutStatusResponse
			{
				SessionId = session.Id,
				IsPaid = isPaid,
				BookingExists = booking != null,
				Status = booking != null
					? "booking_created"
					: isPaid
						? "processing"
						: "unpaid",
				BookingId = booking?.BookingId,
				BookingReference = booking?.BookingReference
			};

			_logger.LogInformation(
				"Checkout status resolved for SessionId {SessionId}. IsPaid {IsPaid}, BookingExists {BookingExists}, Status {Status}",
				sessionId,
				result.IsPaid,
				result.BookingExists,
				result.Status
			);

			return result;
		}

		private StripeWebhookBookingRequest BuildWebhookBookingRequest(Session session)
		{
			if (string.IsNullOrWhiteSpace(session.Id))
			{
				_logger.LogError("Stripe session id missing while building webhook booking request");
				throw new ValidationException("Stripe session id mangler.");
			}

			var metadata = session.Metadata;
			if (metadata == null)
			{
				_logger.LogError(
					"Stripe metadata missing for SessionId {SessionId}",
					session.Id
				);
				throw new ValidationException("Stripe metadata mangler.");
			}

			if (!metadata.TryGetValue("rejseId", out var rejseIdRaw) || !int.TryParse(rejseIdRaw, out var rejseId))
			{
				_logger.LogError(
					"Invalid rejseId in Stripe metadata for SessionId {SessionId}",
					session.Id
				);
				throw new ValidationException("Ugyldig rejseId.");
			}

			if (!metadata.TryGetValue("antalPladser", out var antalPladserRaw) || !int.TryParse(antalPladserRaw, out var antalPladser))
			{
				_logger.LogError(
					"Invalid antalPladser in Stripe metadata for SessionId {SessionId}",
					session.Id
				);
				throw new ValidationException("Ugyldig antalPladser.");
			}

			if (!metadata.TryGetValue("kundeNavn", out var kundeNavn) || string.IsNullOrWhiteSpace(kundeNavn))
			{
				_logger.LogError(
					"Missing kundeNavn in Stripe metadata for SessionId {SessionId}",
					session.Id
				);
				throw new ValidationException("Manglende kundeNavn.");
			}

			if (!metadata.TryGetValue("kundeEmail", out var kundeEmail) || string.IsNullOrWhiteSpace(kundeEmail))
			{
				_logger.LogError(
					"Missing kundeEmail in Stripe metadata for SessionId {SessionId}",
					session.Id
				);
				throw new ValidationException("Manglende kundeEmail.");
			}

			int? userId = null;
			if (metadata.TryGetValue("userId", out var userIdRaw) &&
				!string.IsNullOrWhiteSpace(userIdRaw) &&
				int.TryParse(userIdRaw, out var parsedUserId))
			{
				userId = parsedUserId;
			}

			_logger.LogInformation(
				"Webhook booking request built successfully for SessionId {SessionId}, RejseId {RejseId}",
				session.Id,
				rejseId
			);

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