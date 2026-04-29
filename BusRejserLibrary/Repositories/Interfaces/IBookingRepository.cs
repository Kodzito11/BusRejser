using BusRejserLibrary.Models;

namespace BusRejserLibrary.Repositories
{
	public interface IBookingRepository
	{
		int Create(Booking booking);
		List<Booking> GetAll();
		List<Booking> GetCompletedPaidByUserId(int userId);
		List<Booking> GetCompletedPaidWithRejseByUserId(int userId);
		Booking? GetById(int id);
		Booking? GetByStripeSessionId(string stripeSessionId);
		List<Booking> GetByRejseId(int rejseId);
		int GetTotalBookedSeatsForRejse(int rejseId);
		List<Booking> GetByUserId(int userId);
		bool CancelAndReleaseSeats(int bookingId);
		bool ReactivateAndReserveSeats(int bookingId);
	}
}