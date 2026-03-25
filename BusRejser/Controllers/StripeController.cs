using System.Security.Claims;
using BusRejser.DTOs;
using BusRejser.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace BusRejser.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class StripeController : ControllerBase
	{
		private readonly StripeService _stripeService;

		public StripeController(StripeService stripeService)
		{
			_stripeService = stripeService;
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
				return BadRequest(new ErrorResponse { Message = ex.Message });
			}
		}

		[HttpPost("webhook")]
		[AllowAnonymous]
		public async Task<IActionResult> Webhook()
		{
			var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

			try
			{
				var stripeSignature = Request.Headers["Stripe-Signature"].ToString();
				_stripeService.HandleWebhook(json, stripeSignature);

				return Ok();
			}
			catch (StripeException ex)
			{
				return BadRequest(new ErrorResponse { Message = $"Stripe webhook error: {ex.Message}" });
			}
			catch (Exception ex)
			{
				return BadRequest(new ErrorResponse { Message = ex.Message });
			}
		}
	}
}