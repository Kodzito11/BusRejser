using System.Security.Claims;
using BusRejser.DTOs;
using BusRejser.Services;
using BusRejserLibrary.Models;
using BusRejserLibrary.Repositories;
using BusRejserLibrary.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;


namespace BusRejser.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class StripeController : ControllerBase
	{
		private readonly StripeService _stripeService;
		private readonly IConfiguration _configuration;
		private readonly BookingService _bookingService;
		private readonly BookingRepository _bookingRepository;

		public StripeController(
			StripeService stripeService,
			IConfiguration configuration,
			BookingService bookingService,
			BookingRepository bookingRepository)
		{
			_stripeService = stripeService;
			_configuration = configuration;
			_bookingService = bookingService;
			_bookingRepository = bookingRepository;
		}

		[HttpPost("create-checkout-session")]
		[AllowAnonymous]
		public ActionResult CreateCheckoutSession([FromBody] CreateCheckoutSessionRequest request)
		{
			try
			{
				int? userId = null;

				if (User.Identity?.IsAuthenticated == true)
				{
					var userIdRaw = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
					if (int.TryParse(userIdRaw, out var parsedUserId))
						userId = parsedUserId;
				}

				var origin = Request.Headers.Origin.FirstOrDefault();
				if (string.IsNullOrWhiteSpace(origin))
					origin = "http://localhost:5173";

				var url = _stripeService.CreateCheckoutSession(request, userId, origin);

				return Ok(new { url });
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		[HttpPost("webhook")]
		[AllowAnonymous]
		public async Task<IActionResult> Webhook()
		{
			var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

			try
			{
				var stripeSignature = Request.Headers["Stripe-Signature"];
				var webhookSecret = _configuration["Stripe:WebhookSecret"];

				if (string.IsNullOrWhiteSpace(webhookSecret))
					return BadRequest("Stripe webhook secret mangler.");

				var stripeEvent = EventUtility.ConstructEvent(
					json,
					stripeSignature,
					webhookSecret
				);

				if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
				{
					var session = stripeEvent.Data.Object as Session;

					if (session == null)
						return BadRequest();

					await HandleCheckoutCompleted(session);
				}

				return Ok();
			}
			catch (StripeException ex)
			{
				return BadRequest($"Stripe webhook error: {ex.Message}");
			}
			catch (Exception ex)
			{
				return BadRequest($"Webhook error: {ex.Message}");
			}
		}

		private Task HandleCheckoutCompleted(Session session)
		{
			if (string.IsNullOrWhiteSpace(session.Id))
				throw new Exception("Stripe session id mangler.");

			var existingBooking = _bookingRepository.GetByStripeSessionId(session.Id);
			if (existingBooking != null)
				return Task.CompletedTask;

			var metadata = session.Metadata;
			if (metadata == null)
				throw new Exception("Stripe metadata mangler.");

			if (!metadata.TryGetValue("rejseId", out var rejseIdRaw) || !int.TryParse(rejseIdRaw, out var rejseId))
				throw new Exception("Ugyldig eller manglende rejseId i metadata.");

			if (!metadata.TryGetValue("antalPladser", out var antalPladserRaw) || !int.TryParse(antalPladserRaw, out var antalPladser))
				throw new Exception("Ugyldig eller manglende antalPladser i metadata.");

			if (!metadata.TryGetValue("kundeNavn", out var kundeNavn) || string.IsNullOrWhiteSpace(kundeNavn))
				throw new Exception("Manglende kundeNavn i metadata.");

			if (!metadata.TryGetValue("kundeEmail", out var kundeEmail) || string.IsNullOrWhiteSpace(kundeEmail))
				throw new Exception("Manglende kundeEmail i metadata.");

			int? userId = null;
			if (metadata.TryGetValue("userId", out var userIdRaw) &&
				!string.IsNullOrWhiteSpace(userIdRaw) &&
				int.TryParse(userIdRaw, out var parsedUserId))
			{
				userId = parsedUserId;
			}

			var totalPrice = (session.AmountTotal ?? 0) / 100m;

			var booking = Booking.Create(
				rejseId,
				userId,
				kundeNavn,
				kundeEmail,
				antalPladser,
				totalPrice
			);

			booking.MarkAsPaid(session.Id, session.PaymentIntentId);

			_bookingService.Create(booking);

			return Task.CompletedTask;
		}
	}
}