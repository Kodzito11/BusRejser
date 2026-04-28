using BusRejser.DTOs;
using BusRejser.Exceptions;
using BusRejser.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BusRejser.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class BadgeController : ControllerBase
	{
		private readonly BadgeService _badgeService;

		public BadgeController(BadgeService badgeService)
		{
			_badgeService = badgeService;
		}

		[HttpGet]
		[AllowAnonymous]
		public ActionResult<IEnumerable<BadgeResponse>> GetAll()
		{
			return Ok(_badgeService.GetAllActive());
		}

		[HttpGet("mine")]
		[Authorize(Roles = "Kunde")]
		public ActionResult<IEnumerable<UserBadgeResponse>> GetMine()
		{
			var userId = GetUserId();
			return Ok(_badgeService.GetByUserId(userId));
		}

		[HttpPost("evaluate")]
		[Authorize(Roles = "Kunde")]
		public IActionResult EvaluateMine()
		{
			var userId = GetUserId();
			_badgeService.EvaluateUserBadges(userId);

			return Ok(new { message = "Badges opdateret." });
		}

		private int GetUserId()
		{
			var userIdRaw = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			if (!int.TryParse(userIdRaw, out var userId))
				throw new UnauthorizedException("Ugyldig bruger.");

			return userId;
		}
	}
}