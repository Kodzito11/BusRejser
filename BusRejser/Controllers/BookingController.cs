using BusRejser.DTOs;
using BusRejser.Exceptions;
using BusRejserLibrary.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BusRejser.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class BookingController : ControllerBase
	{
		private readonly BookingService _bookingService;

		public BookingController(BookingService bookingService)
		{
			_bookingService = bookingService;
		}

		[HttpGet]
		[Authorize(Roles = "Admin,Medarbejder")]
		public ActionResult<IEnumerable<BookingResponse>> GetAll()
		{
			return Ok(_bookingService.GetAllResponses());
		}

		[HttpPost]
		[AllowAnonymous]
		public ActionResult Create()
		{
			return BadRequest(new ErrorResponse
			{
				Message = "Direkte booking er ikke længere aktiv. Brug Stripe checkout flow."
			});
		}

		[HttpGet("rejse/{rejseId:int}")]
		[Authorize(Roles = "Admin,Medarbejder")]
		public ActionResult<IEnumerable<BookingResponse>> GetByRejseId(int rejseId)
		{
			return Ok(_bookingService.GetByRejseIdResponses(rejseId));
		}

		[HttpGet("mine")]
		[Authorize(Roles = "Kunde")]
		public ActionResult<IEnumerable<BookingResponse>> GetMine()
		{
			var userIdRaw = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			if (!int.TryParse(userIdRaw, out var userId))
				throw new UnauthorizedException("Ugyldig bruger.");

			return Ok(_bookingService.GetByUserIdResponses(userId));
		}

		[HttpGet("rejse/{rejseId:int}/available-seats")]
		[AllowAnonymous]
		public ActionResult<int> GetAvailableSeats(int rejseId)
		{
			return Ok(_bookingService.GetAvailableSeats(rejseId));
		}

		[HttpPut("{id:int}/cancel")]
		[Authorize]
		public ActionResult Cancel(int id)
		{
			var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
			var isStaff = role == "Admin" || role == "Medarbejder";

			int? userId = null;

			if (!isStaff)
			{
				var userIdRaw = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

				if (!int.TryParse(userIdRaw, out var parsedUserId))
					throw new UnauthorizedException("Ugyldig bruger.");

				userId = parsedUserId;
			}

			_bookingService.Cancel(id, userId, isStaff);

			return Ok();
		}

		[HttpPut("{id:int}/reactivate")]
		[Authorize(Roles = "Admin,Medarbejder")]
		public ActionResult Reactivate(int id)
		{
			_bookingService.Reactivate(id);
			return Ok();
		}
	}
}