using BusRejser.DTOs;
using BusRejser.Exceptions;
using BusRejser.Mappers;
using BusRejserLibrary.Enums;
using BusRejserLibrary.Models;
using BusRejserLibrary.Repositories;

namespace BusRejserLibrary.Services
{
	public class BookingService
	{
		private readonly BookingRepository _bookingRepository;
		private readonly RejseRepository _rejseRepository;
		private readonly UserRepository _userRepository;
		private readonly ILogger<BookingService> _logger;

		public BookingService(
			BookingRepository bookingRepository,
			RejseRepository rejseRepository,
			UserRepository userRepository,
			ILogger<BookingService> logger)
		{
			_bookingRepository = bookingRepository;
			_rejseRepository = rejseRepository;
			_userRepository = userRepository;
			_logger = logger;
		}

		public List<BookingResponse> GetAllResponses()
		{
			var bookings = _bookingRepository.GetAll();

			return bookings
				.Select(ToResponse)
				.ToList();
		}

		public BookingResponse? GetResponseById(int id)
		{
			var booking = _bookingRepository.GetById(id);
			if (booking == null) return null;

			return ToResponse(booking);
		}

		public List<BookingResponse> GetByRejseIdResponses(int rejseId)
		{
			var bookings = _bookingRepository.GetByRejseId(rejseId);

			return bookings
				.Select(ToResponse)
				.ToList();
		}

		public List<BookingResponse> GetByUserIdResponses(int userId)
		{
			var bookings = _bookingRepository.GetByUserId(userId);

			return bookings
				.Select(ToResponse)
				.ToList();
		}

		private BookingResponse ToResponse(Booking booking)
		{
			var role = GetUserRole(booking.UserId);
			return BookingMapper.ToResponse(booking, role);
		}

		private string? GetUserRole(int? userId)
		{
			if (userId == null)
				return null;

			var user = _userRepository.GetById(userId.Value);
			return user?.Role.ToString();
		}

		public int Create(Booking booking)
		{
			var rejse = _rejseRepository.GetById(booking.RejseId);
			if (rejse == null)
				throw new NotFoundException("Rejse findes ikke.");

			if (booking.Status != BookingStatus.Paid)
				throw new ValidationException("Kun betalte bookinger kan oprettes.");

			var reserved = _rejseRepository.TryReserveSeats(booking.RejseId, booking.AntalPladser);
			if (!reserved)
				throw new ValidationException("Ikke nok ledige pladser.");

			try
			{
				return _bookingRepository.Create(booking);
			}
			catch
			{
				_rejseRepository.ReleaseSeats(booking.RejseId, booking.AntalPladser);
				throw;
			}
		}

		public int GetAvailableSeats(int rejseId)
		{
			var rejse = _rejseRepository.GetById(rejseId);
			if (rejse == null)
				throw new NotFoundException("Rejse findes ikke.");

			return rejse.MaxSeats - rejse.BookedSeats;
		}

		public bool Cancel(int bookingId, int? actingUserId, bool isStaff)
		{
			var booking = _bookingRepository.GetById(bookingId);
			if (booking == null)
				throw new NotFoundException("Booking blev ikke fundet.");

			if (booking.Status == BookingStatus.Cancelled)
				return true;

			if (booking.Status != BookingStatus.Paid)
				throw new ValidationException("Kun betalte bookinger kan annulleres.");

			if (!isStaff)
			{
				if (!actingUserId.HasValue)
					throw new ValidationException("Ugyldig bruger.");

				if (booking.UserId != actingUserId.Value)
					throw new ForbiddenException("Du må kun annullere dine egne bookinger.");

				var rejse = _rejseRepository.GetById(booking.RejseId);
				if (rejse == null)
					throw new NotFoundException("Rejse findes ikke.");

				if (rejse.StartAt <= DateTime.UtcNow.AddHours(24))
					throw new ValidationException("Booking kan kun annulleres senest 24 timer før afgang.");
			}

			var cancelled = _bookingRepository.CancelAndReleaseSeats(bookingId);
			if (!cancelled)
				throw new ConflictException("Booking kunne ikke annulleres.");

			return true;
		}

		public bool Reactivate(int bookingId)
		{
			var booking = _bookingRepository.GetById(bookingId);
			if (booking == null)
				throw new NotFoundException("Booking blev ikke fundet.");

			if (booking.Status == BookingStatus.Paid)
				return true;

			if (booking.Status != BookingStatus.Cancelled)
				throw new ValidationException("Kun annullerede bookinger kan genaktiveres.");

			var reactivated = _bookingRepository.ReactivateAndReserveSeats(bookingId);
			if (!reactivated)
				throw new ConflictException("Booking kunne ikke genaktiveres.");

			return true;
		}

		public void CreateFromStripe(StripeWebhookBookingRequest request)
		{
			var existing = _bookingRepository.GetByStripeSessionId(request.StripeSessionId);
			if (existing != null)
				return;

			var booking = Booking.Create(
				request.RejseId,
				request.UserId,
				request.KundeNavn,
				request.KundeEmail,
				request.AntalPladser,
				request.TotalPrice
			);

			booking.MarkAsPaid(request.StripeSessionId, request.StripePaymentIntentId);

			Create(booking);
		}


	}
}