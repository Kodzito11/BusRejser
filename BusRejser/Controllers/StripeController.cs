using System.Security.Claims;
using BusRejser.DTOs;
using BusRejser.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

				_logger.LogInformation(
					"Checkout session created successfully for RejseId {RejseId}",
					request.RejseId
				);

				return Ok(new { url });
			}
			catch (Exception ex)
			{
				_logger.LogError(
					ex,
					"Error creating checkout session for RejseId {RejseId}",
					request.RejseId
				);

				return StatusCode(500, "An error occurred while creating the checkout session.");
			}
		}

		[HttpPost("webhook")]
		[AllowAnonymous]
		public async Task<IActionResult> Webhook()
		{
			_logger.LogInformation("Stripe webhook endpoint called");

			try
			{
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
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error processing Stripe webhook");
				return StatusCode(500, ex.Message);
			}
		}

		[HttpGet("checkout-status")]
		[AllowAnonymous]
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

			try
			{
				var result = _stripeService.GetCheckoutStatus(sessionId);

				_logger.LogInformation(
					"Checkout status returned for session {SessionId} with status {Status}",
					sessionId,
					result.Status
				);

				return Ok(result);
			}
			catch (Exception ex)
			{
				_logger.LogError(
					ex,
					"Error getting checkout status for session {SessionId}",
					sessionId
				);

				return StatusCode(500, "An error occurred while getting checkout status.");
			}
		}
	}
}