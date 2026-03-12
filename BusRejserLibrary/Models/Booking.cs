using BusRejserLibrary.Enums;
using Microsoft.Extensions.Configuration.UserSecrets;
using System.Security.Cryptography;
using System.Text;

namespace BusRejserLibrary.Models
{
	public class Booking
	{
		public int BookingId { get; set; }
		public int RejseId { get; private set; }
		public int? UserId { get; private set; }
		public string KundeNavn { get; private set; }
		public string KundeEmail { get; private set; }
		public int AntalPladser { get; private set; }
		public DateTime CreatedAt { get; private set; }
		public BookingStatus Status { get; private set; }
		public string BookingReference { get; private set; } = "";

		private Booking(int rejseId, int? userId, string kundeNavn, string kundeEmail, int antalPladser)
		{
			RejseId = rejseId;
			UserId = userId;
			KundeNavn = kundeNavn;
			KundeEmail = kundeEmail;
			AntalPladser = antalPladser;
			CreatedAt = DateTime.UtcNow;
			Status = BookingStatus.Aktiv;
			BookingReference = GenerateReference();
		}

		public static Booking Restore(
			int bookingId,
			int rejseId,
			int? userId,
			string bookingReference,
			string kundeNavn,
			string kundeEmail,
			int antalPladser,
			BookingStatus status,
			DateTime createdAt)
		{
			var booking = new Booking(rejseId, userId, kundeNavn, kundeEmail, antalPladser);
			booking.BookingId = bookingId;
			booking.BookingReference = bookingReference;
			booking.Status = status;
			booking.CreatedAt = createdAt;
			return booking;
		}


	private static string GenerateReference()
	{
		const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
		const int length = 8;

		var bytes = RandomNumberGenerator.GetBytes(length);
		var sb = new StringBuilder(length);

		for (int i = 0; i < length; i++)
		{
			sb.Append(chars[bytes[i] % chars.Length]);
		}

		return $"BP-{sb}";
	}

	public static Booking Create(int rejseId, int? userId, string kundeNavn, string kundeEmail, int antalPladser)
		{
			if (rejseId <= 0)
				throw new ArgumentOutOfRangeException(nameof(rejseId));

			if (userId.HasValue && userId <= 0)
				throw new ArgumentOutOfRangeException(nameof(userId));

			if (string.IsNullOrWhiteSpace(kundeNavn))
				throw new ArgumentException("Kundenavn kræves.");

			if (string.IsNullOrWhiteSpace(kundeEmail))
				throw new ArgumentException("Kundeemail kræves.");

			if (antalPladser <= 0)
				throw new ArgumentOutOfRangeException(nameof(antalPladser));

			return new Booking(rejseId, userId, kundeNavn, kundeEmail, antalPladser);
		}

		public void Cancel()
		{
			Status = BookingStatus.Annulleret;
		}
	}
}