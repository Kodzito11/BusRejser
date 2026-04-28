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
	[Authorize]
	public class TravelHistoryController : ControllerBase
	{
		private readonly TravelHistoryService _travelHistoryService;

		public TravelHistoryController(TravelHistoryService travelHistoryService)
		{
			_travelHistoryService = travelHistoryService;
		}

		[HttpGet("mine")]
		[Authorize(Roles = "Kunde")]
		public ActionResult<IEnumerable<TravelHistoryResponse>> GetMine()
		{
			var userId = GetUserId();
			return Ok(_travelHistoryService.GetByUserId(userId));
		}

		[HttpPost("sync")]
		[Authorize(Roles = "Kunde")]
		public IActionResult SyncMine()
		{
			var userId = GetUserId();
			_travelHistoryService.SyncCompletedTripsForUser(userId);

			return Ok(new { message = "Rejsehistorik opdateret." });
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