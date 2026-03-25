using BusRejser.DTOs;
using BusRejserLibrary.Models;

namespace BusRejser.Mappers
{
	public static class BookingMapper
	{
		public static BookingResponse ToResponse(Booking booking, string? role = null)
		{
			return new BookingResponse
			{
				BookingId = booking.BookingId,
				RejseId = booking.RejseId,
				UserId = booking.UserId,
				Role = role,
				KundeNavn = booking.KundeNavn,
				KundeEmail = booking.KundeEmail,
				AntalPladser = booking.AntalPladser,
				CreatedAt = booking.CreatedAt,
				Status = (int)booking.Status,
				BookingReference = booking.BookingReference,
				TotalPrice = booking.TotalPrice,
				PaidAt = booking.PaidAt
			};
		}
	}
}