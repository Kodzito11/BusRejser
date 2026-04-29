using BusRejserLibrary.Database;
using BusRejserLibrary.Enums;
using BusRejserLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace BusRejserLibrary.Repositories
{
	public class BookingRepository : IBookingRepository
	{
		private readonly BusPlanenDbContext _context;

		public BookingRepository(BusPlanenDbContext context)
		{
			_context = context;
		}

		public int Create(Booking booking)
		{
			if (booking == null)
				throw new ArgumentNullException(nameof(booking));

			_context.Bookings.Add(booking);
			_context.SaveChanges();

			return booking.BookingId;
		}

		public List<Booking> GetAll()
		{
			return _context.Bookings
				.AsNoTracking()
				.OrderByDescending(x => x.BookingId)
				.ToList();
		}

		public Booking? GetById(int id)
		{
			return _context.Bookings
				.AsNoTracking()
				.FirstOrDefault(x => x.BookingId == id);
		}

		public Booking? GetByStripeSessionId(string stripeSessionId)
		{
			return _context.Bookings
				.AsNoTracking()
				.FirstOrDefault(x => x.StripeSessionId == stripeSessionId);
		}

		public List<Booking> GetByRejseId(int rejseId)
		{
			return _context.Bookings
				.AsNoTracking()
				.Where(x => x.RejseId == rejseId)
				.OrderByDescending(x => x.CreatedAt)
				.ToList();
		}

		public List<Booking> GetByUserId(int userId)
		{
			return _context.Bookings
				.AsNoTracking()
				.Where(x => x.UserId == userId)
				.OrderByDescending(x => x.CreatedAt)
				.ToList();
		}

		public int GetTotalBookedSeatsForRejse(int rejseId)
		{
			return _context.Bookings
				.Where(x => x.RejseId == rejseId && x.Status == BookingStatus.Paid)
				.Sum(x => (int?)x.AntalPladser) ?? 0;
		}

		public bool CancelAndReleaseSeats(int bookingId)
		{
			using var transaction = _context.Database.BeginTransaction();

			try
			{
				var booking = _context.Bookings
					.FirstOrDefault(x => x.BookingId == bookingId);

				if (booking == null)
					return false;

				if (booking.Status == BookingStatus.Cancelled)
					return true;

				if (booking.Status != BookingStatus.Paid)
					return false;

				var rejse = _context.Rejser
					.FirstOrDefault(x => x.RejseId == booking.RejseId);

				if (rejse == null)
					return false;

				if (rejse.BookedSeats < booking.AntalPladser)
					return false;

				booking.Cancel();
				rejse.BookedSeats -= booking.AntalPladser;

				_context.SaveChanges();
				transaction.Commit();

				return true;
			}
			catch
			{
				transaction.Rollback();
				throw;
			}
		}

		public bool ReactivateAndReserveSeats(int bookingId)
		{
			using var transaction = _context.Database.BeginTransaction();

			try
			{
				var booking = _context.Bookings
					.FirstOrDefault(x => x.BookingId == bookingId);

				if (booking == null)
					return false;

				if (booking.Status == BookingStatus.Paid)
					return true;

				if (booking.Status != BookingStatus.Cancelled)
					return false;

				var rejse = _context.Rejser
					.FirstOrDefault(x => x.RejseId == booking.RejseId);

				if (rejse == null)
					return false;

				if (rejse.BookedSeats + booking.AntalPladser > rejse.MaxSeats)
					return false;

				rejse.BookedSeats += booking.AntalPladser;
				booking.MarkAsPaid(booking.StripeSessionId, booking.StripePaymentIntentId);

				_context.SaveChanges();
				transaction.Commit();

				return true;
			}
			catch
			{
				transaction.Rollback();
				throw;
			}
		}

		public List<Booking> GetCompletedPaidByUserId(int userId)
		{
			return _context.Bookings
				.AsNoTracking()
				.Where(x =>
					x.UserId == userId &&
					x.Status == BookingStatus.Paid)
				.ToList();
		}
	}
}