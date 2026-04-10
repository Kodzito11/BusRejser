using BusRejser.DTOs;
using BusRejser.Exceptions;
using BusRejser.Mappers;
using BusRejserLibrary.Enums;
using BusRejserLibrary.Models;
using BusRejserLibrary.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BusRejserLibrary.Services
{
	public class BookingService
	{
		private readonly IBookingRepository _bookingRepository;
		private readonly IRejseRepository _rejseRepository;
		private readonly IUserRepository _userRepository;
		private readonly ILogger<BookingService> _logger;

		public BookingService(
			IBookingRepository bookingRepository,
			IRejseRepository rejseRepository,
			IUserRepository userRepository,
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
			{
				_logger.LogWarning("Booking create failed because RejseId {RejseId} was not found", booking.RejseId);
				throw new NotFoundException("Rejse findes ikke.");
			}

			if (booking.Status != BookingStatus.Paid)
			{
				_logger.LogWarning(
					"Booking create failed because booking for RejseId {RejseId} was not paid. Status {Status}",
					booking.RejseId,
					booking.Status
				);

				throw new ValidationException("Kun betalte bookinger kan oprettes.");
			}

			_logger.LogInformation(
				"Creating booking for RejseId {RejseId} with {Seats} seats",
				booking.RejseId,
				booking.AntalPladser);

			var reserved = _rejseRepository.TryReserveSeats(booking.RejseId, booking.AntalPladser);
			if (!reserved)
			{
				_logger.LogWarning(
					"Booking failed - not enough seats for RejseId {RejseId}",
					booking.RejseId);

				throw new ValidationException("Ikke nok ledige pladser.");
			}

			try
			{
				var bookingId = _bookingRepository.Create(booking);

				_logger.LogInformation(
					"Booking {BookingId} created successfully for RejseId {RejseId}",
					bookingId,
					booking.RejseId);

				return bookingId;
			}
			catch (Exception ex)
			{
				_logger.LogWarning(
					ex,
					"Booking create failed after seat reservation for RejseId {RejseId}. Releasing seats.",
					booking.RejseId);

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
			_logger.LogInformation("Cancelling booking {BookingId}", bookingId);

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
				{
					_logger.LogWarning(
						"User {UserId} tried to cancel booking {BookingId} without permission",
						actingUserId,
						bookingId);

					throw new ForbiddenException("Du må kun annullere dine egne bookinger.");
				}

				var rejse = _rejseRepository.GetById(booking.RejseId);
				if (rejse == null)
					throw new NotFoundException("Rejse findes ikke.");

				if (rejse.StartAt <= DateTime.UtcNow.AddHours(24))
				{
					_logger.LogWarning(
						"Cancel denied for booking {BookingId} because departure is within 24 hours",
						bookingId);

					throw new ValidationException("Booking kan kun annulleres senest 24 timer før afgang.");
				}
			}

			var cancelled = _bookingRepository.CancelAndReleaseSeats(bookingId);
			if (!cancelled)
			{
				_logger.LogWarning("Cancel failed for booking {BookingId}", bookingId);
				throw new ConflictException("Booking kunne ikke annulleres.");
			}

			_logger.LogInformation("Booking {BookingId} cancelled successfully", bookingId);
			return true;
		}

		public bool Reactivate(int bookingId)
		{
			_logger.LogInformation("Reactivating booking {BookingId}", bookingId);

			var booking = _bookingRepository.GetById(bookingId);
			if (booking == null)
				throw new NotFoundException("Booking blev ikke fundet.");

			if (booking.Status == BookingStatus.Paid)
				return true;

			if (booking.Status != BookingStatus.Cancelled)
				throw new ValidationException("Kun annullerede bookinger kan genaktiveres.");

			var reactivated = _bookingRepository.ReactivateAndReserveSeats(bookingId);
			if (!reactivated)
			{
				_logger.LogWarning("Reactivate failed for booking {BookingId}", bookingId);
				throw new ConflictException("Booking kunne ikke genaktiveres.");
			}

			_logger.LogInformation("Booking {BookingId} reactivated successfully", bookingId);
			return true;
		}

		public BookingResponse? GetByStripeSessionId(string stripeSessionId)
		{
			if (string.IsNullOrWhiteSpace(stripeSessionId))
			{
				_logger.LogWarning("GetByStripeSessionId called with empty session id");
				return null;
			}

			var booking = _bookingRepository.GetByStripeSessionId(stripeSessionId);
			if (booking == null)
			{
				_logger.LogInformation(
					"No booking found for Stripe session {SessionId}",
					stripeSessionId
				);
				return null;
			}

			_logger.LogInformation(
				"Booking found for Stripe session {SessionId}. BookingId {BookingId}",
				stripeSessionId,
				booking.BookingId
			);

			return ToResponse(booking);
		}

		public void CreateFromStripe(StripeWebhookBookingRequest request)
		{
			if (request == null)
			{
				_logger.LogWarning("CreateFromStripe called with null request");
				throw new ValidationException("Stripe request må ikke være null.");
			}

			if (string.IsNullOrWhiteSpace(request.StripeSessionId))
			{
				_logger.LogWarning("CreateFromStripe called with missing Stripe session id");
				throw new ValidationException("Stripe session id mangler.");
			}

			_logger.LogInformation(
				"CreateFromStripe started for SessionId {SessionId}, RejseId {RejseId}, Seats {Seats}",
				request.StripeSessionId,
				request.RejseId,
				request.AntalPladser
			);

			var existing = _bookingRepository.GetByStripeSessionId(request.StripeSessionId);
			if (existing != null)
			{
				_logger.LogInformation(
					"Stripe booking ignored - already exists for session {SessionId} with BookingId {BookingId}",
					request.StripeSessionId,
					existing.BookingId
				);

				return;
			}

			var booking = Booking.Create(
				request.RejseId,
				request.UserId,
				request.KundeNavn,
				request.KundeEmail,
				request.AntalPladser,
				request.TotalPrice
			);

			booking.MarkAsPaid(request.StripeSessionId, request.StripePaymentIntentId);

			_logger.LogInformation(
				"Booking marked as paid for Stripe session {SessionId}. Creating booking in repository flow",
				request.StripeSessionId
			);

			try
			{
				Create(booking);
			}
			catch (Exception ex) when (ex is DbUpdateException || ex is InvalidOperationException || ex is ConflictException)
			{
				var duplicate = _bookingRepository.GetByStripeSessionId(request.StripeSessionId);
				if (duplicate == null)
				{
					throw;
				}

				_logger.LogWarning(
					ex,
					"Duplicate or concurrent Stripe booking detected for SessionId {SessionId}. Existing BookingId {BookingId} will be kept.",
					request.StripeSessionId,
					duplicate.BookingId
				);

				return;
			}

			_logger.LogInformation(
				"CreateFromStripe completed successfully for SessionId {SessionId}",
				request.StripeSessionId
			);
		}
	}
}
