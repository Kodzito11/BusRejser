using System.Security.Claims;
using BusRejserLibrary.Models;
using BusRejserLibrary.Services;
using BusRejserLibrary.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BusRejser.DTOs;

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

				var result = bookings.Select(b =>
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
						BookingReference = b.BookingReference
					};
				});

				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

		[HttpPost]
		[AllowAnonymous]
		public ActionResult Create([FromBody] BookingCreateRequest request)
		{
			try
			{
				var isAuthenticated = User.Identity?.IsAuthenticated == true;

				int? userId = null;
				string kundeNavn;
				string kundeEmail;

				if (isAuthenticated)
				{
					var userIdRaw = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
					if (!int.TryParse(userIdRaw, out var parsedUserId))
						return Unauthorized(new { Message = "Ugyldig bruger." });

					userId = parsedUserId;
					kundeNavn = User.FindFirst(ClaimTypes.Name)?.Value ?? "";
					kundeEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";

					if (string.IsNullOrWhiteSpace(kundeNavn) || string.IsNullOrWhiteSpace(kundeEmail))
						return BadRequest(new { Message = "Brugeroplysninger mangler i token." });
				}
				else
				{
					kundeNavn = request.KundeNavn;
					kundeEmail = request.KundeEmail;
				}

				var booking = Booking.Create(
					request.RejseId,
					userId,
					kundeNavn,
					kundeEmail,
					request.AntalPladser
				);

				var id = _bookingService.Create(booking);

				return Ok(new
				{
					bookingId = id,
					bookingReference = booking.BookingReference
				});
			}
			catch (Exception ex)
			{
				return BadRequest(new { Message = ex.Message });
			}
		}

		[HttpGet("rejse/{rejseId:int}")]
		[Authorize(Roles = "Admin,Medarbejder")]
		public ActionResult<IEnumerable<BookingResponse>> GetByRejseId(int rejseId)
		{
			try
			{
				var bookings = _bookingService.GetByRejseId(rejseId);

				var result = bookings.Select(b =>
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
						BookingReference = b.BookingReference
					};
				});

				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

		[HttpGet("mine")]
		[Authorize (Roles = "Kunde")]
		public ActionResult<List<Booking>> GetMine()
		{
			var userIdRaw = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			if (!int.TryParse(userIdRaw, out var userId))
				return Unauthorized(new { Message = "Ugyldig bruger." });

			return _bookingService.GetByUserId(userId);
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
	}

	public class BookingCreateRequest
	{
		public int RejseId { get; set; }
		public string KundeNavn { get; set; } = "";
		public string KundeEmail { get; set; } = "";
		public int AntalPladser { get; set; }
	}
}