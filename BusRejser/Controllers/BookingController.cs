using System.Security.Claims;
using BusRejser.DTOs;
using BusRejserLibrary.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
			try
			{
				return Ok(_bookingService.GetAllResponses());
			}
			catch (Exception ex)
			{
				return BadRequest(Error(ex.Message));
			}
		}

		[HttpPost]
		[AllowAnonymous]
		public ActionResult Create()
		{
			return BadRequest(Error("Direkte booking er ikke længere aktiv. Brug Stripe checkout flow."));
		}

		[HttpGet("rejse/{rejseId:int}")]
		[Authorize(Roles = "Admin,Medarbejder")]
		public ActionResult<IEnumerable<BookingResponse>> GetByRejseId(int rejseId)
		{
			try
			{
				return Ok(_bookingService.GetByRejseIdResponses(rejseId));
			}
			catch (Exception ex)
			{
				return BadRequest(Error(ex.Message));
			}
		}

		[HttpGet("mine")]
		[Authorize(Roles = "Kunde")]
		public ActionResult<IEnumerable<BookingResponse>> GetMine()
		{
			try
			{
				var userIdRaw = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

				if (!int.TryParse(userIdRaw, out var userId))
					return Unauthorized(Error("Ugyldig bruger."));

				return Ok(_bookingService.GetByUserIdResponses(userId));
			}
			catch (Exception ex)
			{
				return BadRequest(Error(ex.Message));
			}
		}

		[HttpGet("rejse/{rejseId:int}/available-seats")]
		[AllowAnonymous]
		public ActionResult<int> GetAvailableSeats(int rejseId)
		{
			try
			{
				return Ok(_bookingService.GetAvailableSeats(rejseId));
			}
			catch (Exception ex)
			{
				return NotFound(Error(ex.Message));
			}
		}

		[HttpPut("{id:int}/cancel")]
		[Authorize]
		public ActionResult Cancel(int id)
		{
			try
			{
				var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
				var isStaff = role == "Admin" || role == "Medarbejder";

				int? userId = null;
				if (!isStaff)
				{
					var userIdRaw = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
					if (!int.TryParse(userIdRaw, out var parsedUserId))
						return Unauthorized(Error("Ugyldig bruger."));

					userId = parsedUserId;
				}

				var ok = _bookingService.Cancel(id, userId, isStaff);
				return ok ? Ok() : NotFound(Error("Booking blev ikke fundet."));
			}
			catch (Exception ex)
			{
				return BadRequest(Error(ex.Message));
			}
		}

		[HttpPut("{id:int}/reactivate")]
		[Authorize(Roles = "Admin,Medarbejder")]
		public ActionResult Reactivate(int id)
		{
			try
			{
				var ok = _bookingService.Reactivate(id);
				return ok ? Ok() : NotFound(Error("Booking blev ikke fundet."));
			}
			catch (Exception ex)
			{
				return BadRequest(Error(ex.Message));
			}
		}

		private static ErrorResponse Error(string message)
		{
			return new ErrorResponse { Message = message };
		}
	}
}