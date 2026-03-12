using BusRejserLibrary.Models;
using BusRejserLibrary.Repositories;
using BusRejserLibrary.Enums;

namespace BusRejserLibrary.Services
{
	public class BookingService
	{
		private readonly BookingRepository _bookingRepository;
		private readonly RejseRepository _rejseRepository;

		public BookingService(BookingRepository bookingRepository, RejseRepository rejseRepository)
		{
			_bookingRepository = bookingRepository;
			_rejseRepository = rejseRepository;
		}

		public int Create(Booking booking)
		{
			var rejse = _rejseRepository.GetById(booking.RejseId);
			if (rejse == null)
				throw new Exception("Rejse findes ikke.");

			var reserved = _rejseRepository.TryReserveSeats(booking.RejseId, booking.AntalPladser);
			if (!reserved)
				throw new Exception("Ikke nok ledige pladser.");

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

		public Booking? GetById(int id) => _bookingRepository.GetById(id);

		public List<Booking> GetByRejseId(int rejseId) => _bookingRepository.GetByRejseId(rejseId);

		public int GetAvailableSeats(int rejseId)
		{
			var rejse = _rejseRepository.GetById(rejseId);
			if (rejse == null)
				throw new Exception("Rejse findes ikke.");

			return rejse.MaxSeats - rejse.BookedSeats;
		}

		public List<Booking> GetByUserId(int userId) => _bookingRepository.GetByUserId(userId);

		public bool Cancel(int bookingId)
		{
			var booking = _bookingRepository.GetById(bookingId);
			if (booking == null)
				return false;

			if (booking.Status == BookingStatus.Annulleret)
				return true;

			var cancelled = _bookingRepository.Cancel(bookingId);
			if (!cancelled)
				return false;

			return _rejseRepository.ReleaseSeats(booking.RejseId, booking.AntalPladser);
		}

		public bool Cancel(int bookingId, int? actingUserId, bool isStaff)
		{
			var booking = _bookingRepository.GetById(bookingId);
			if (booking == null)
				return false;

			if (booking.Status == BookingStatus.Annulleret)
				return true;

			if (!isStaff)
			{
				if (!actingUserId.HasValue)
					throw new Exception("Ugyldig bruger.");

				if (booking.UserId != actingUserId.Value)
					throw new Exception("Du må kun annullere dine egne bookinger.");

				var rejse = _rejseRepository.GetById(booking.RejseId);
				if (rejse == null)
					throw new Exception("Rejse findes ikke.");

				if (rejse.StartAt <= DateTime.UtcNow.AddHours(24))
					throw new Exception("Booking kan kun annulleres senest 24 timer før afgang.");
			}

			var cancelled = _bookingRepository.Cancel(bookingId);
			if (!cancelled)
				return false;

			return _rejseRepository.ReleaseSeats(booking.RejseId, booking.AntalPladser);
		}

		public bool Reactivate(int bookingId)
		{
			var booking = _bookingRepository.GetById(bookingId);
			if (booking == null)
				return false;

			if (booking.Status == BookingStatus.Aktiv)
				return true;

			var reserved = _rejseRepository.TryReserveSeats(booking.RejseId, booking.AntalPladser);
			if (!reserved)
				throw new Exception("Ikke nok ledige pladser til at genaktivere bookingen.");

			try
			{
				return _bookingRepository.Reactivate(bookingId);
			}
			catch
			{
				_rejseRepository.ReleaseSeats(booking.RejseId, booking.AntalPladser);
				throw;
			}
		}
	}
}