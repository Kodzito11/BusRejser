using BusRejser.Exceptions;
using BusRejser.Features.Progression.DTOs;
using BusRejser.Features.Progression.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BusRejser.Features.Progression
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize(Roles = "Kunde")]
	public class ProgressionController : ControllerBase
	{
		private readonly ProgressionService _progressionService;

		public ProgressionController(ProgressionService progressionService)
		{
			_progressionService = progressionService;
		}

		[HttpGet("map")]
		public ActionResult<ProgressionMapResponse> GetMap()
		{
			var userId = GetUserId();
			return Ok(_progressionService.GetMap(userId));
		}

		[HttpPost("sync")]
		public IActionResult Sync()
		{
			var userId = GetUserId();
			_progressionService.SyncUserProgress(userId);

			return Ok(new { message = "Progression opdateret." });
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