using System.Security.Claims;
using BusRejser.DTOs;
using BusRejserLibrary.Models;
using BusRejserLibrary.Repositories;
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
		private readonly UserRepository _userRepository;

		public BookingController(BookingService bookingService, UserRepository userRepository)
		{
			_bookingService = bookingService;
			_userRepository = userRepository;
		}

		[HttpGet]
		[Authorize(Roles = "Admin,Medarbejder")]
		public ActionResult<IEnumerable<BookingResponse>> GetAll()
		{
			try
			{
				var bookings = _bookingService.GetAll();

				var result = bookings.Select(MapToResponse);

				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(new { Message = ex.Message });
			}
		}

		[HttpPost]
		[AllowAnonymous]
		public ActionResult Create()
		{
			return BadRequest(new
			{
				Message = "Direkte booking er ikke længere aktiv. Brug Stripe checkout flow."
			});
		}

		[HttpGet("rejse/{rejseId:int}")]
		[Authorize(Roles = "Admin,Medarbejder")]
		public ActionResult<IEnumerable<BookingResponse>> GetByRejseId(int rejseId)
		{
			try
			{
				var bookings = _bookingService.GetByRejseId(rejseId);

				var result = bookings.Select(MapToResponse);

				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(new { Message = ex.Message });
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
					return Unauthorized(new { Message = "Ugyldig bruger." });

				var bookings = _bookingService.GetByUserId(userId);
				var result = bookings.Select(MapToResponse);

				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(new { Message = ex.Message });
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
				return NotFound(new { Message = ex.Message });
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
						return Unauthorized(new { Message = "Ugyldig bruger." });

					userId = parsedUserId;
				}

				var ok = _bookingService.Cancel(id, userId, isStaff);
				return ok ? Ok() : NotFound();
			}
			catch (Exception ex)
			{
				return BadRequest(new { Message = ex.Message });
			}
		}

		[HttpPut("{id:int}/reactivate")]
		[Authorize(Roles = "Admin,Medarbejder")]
		public ActionResult Reactivate(int id)
		{
			try
			{
				var ok = _bookingService.Reactivate(id);
				return ok ? Ok() : NotFound();
			}
			catch (Exception ex)
			{
				return BadRequest(new { Message = ex.Message });
			}
		}

		private BookingResponse MapToResponse(Booking b)
		{
			var user = b.UserId != null
				? _userRepository.GetById(b.UserId.Value)
				: null;

			return new BookingResponse
			{
				BookingId = b.BookingId,
				RejseId = b.RejseId,
				UserId = b.UserId,
				Role = user != null ? user.Role.ToString() : null,
				KundeNavn = b.KundeNavn,
				KundeEmail = b.KundeEmail,
				AntalPladser = b.AntalPladser,
				CreatedAt = b.CreatedAt,
				Status = (int)b.Status,
				BookingReference = b.BookingReference,

				// tilføj kun hvis de findes i din DTO:
				//TotalPrice = b.TotalPrice,
				//PaidAt = b.PaidAt
			};
		}
	}
}