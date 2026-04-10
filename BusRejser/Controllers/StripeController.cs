using System.Security.Claims;
using BusRejser.DTOs;
using BusRejser.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BusRejser.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class StripeController : ControllerBase
	{
		private readonly StripeService _stripeService;
		private readonly ILogger<StripeController> _logger;

		public StripeController(StripeService stripeService, ILogger<StripeController> logger)
		{
			_stripeService = stripeService;
			_logger = logger;
		}

		[HttpPost("create-checkout-session")]
		[AllowAnonymous]
		[EnableRateLimiting("payment-checkout-create")]
		public ActionResult CreateCheckoutSession([FromBody] CreateCheckoutSessionRequest request)
		{
			if (request == null)
			{
				_logger.LogWarning("Create checkout session called with null request");
				return BadRequest();
			}

			_logger.LogInformation(
				"CreateCheckoutSession called for RejseId: {RejseId}, AntalPladser: {AntalPladser}",
				request.RejseId,
				request.AntalPladser
			);

			int? userId = null;

			if (User.Identity?.IsAuthenticated == true)
			{
				var userIdRaw = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
				if (int.TryParse(userIdRaw, out var parsedUserId))
					userId = parsedUserId;
			}

			var url = _stripeService.CreateCheckoutSession(request, userId);

			_logger.LogInformation(
				"Checkout session created successfully for RejseId {RejseId}",
				request.RejseId
			);

			return Ok(new { url });
		}

		[HttpPost("webhook")]
		[AllowAnonymous]
		public async Task<IActionResult> Webhook()
		{
			_logger.LogInformation("Stripe webhook endpoint called");

			var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
			var stripeSignature = Request.Headers["Stripe-Signature"].ToString();

			_logger.LogInformation(
				"Stripe webhook received. SignaturePresent: {SignaturePresent}, BodyLength: {BodyLength}",
				!string.IsNullOrWhiteSpace(stripeSignature),
				json.Length
			);

			_stripeService.HandleWebhook(json, stripeSignature);

			_logger.LogInformation("Stripe webhook processed successfully");

			return Ok();
		}

		[HttpGet("checkout-status")]
		[AllowAnonymous]
		[EnableRateLimiting("payment-checkout-status")]
		public ActionResult<CheckoutStatusResponse> GetCheckoutStatus([FromQuery] string sessionId)
		{
			if (string.IsNullOrWhiteSpace(sessionId))
			{
				_logger.LogWarning("Checkout status called with missing sessionId");
				return BadRequest("sessionId is required");
			}

			_logger.LogInformation(
				"Checkout status requested for session {SessionId}",
				sessionId
			);

			var result = _stripeService.GetCheckoutStatus(sessionId);

			_logger.LogInformation(
				"Checkout status returned for session {SessionId} with status {Status}",
				sessionId,
				result.Status
			);

			return Ok(result);
		}
	}
}
