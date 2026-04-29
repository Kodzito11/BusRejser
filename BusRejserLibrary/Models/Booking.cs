using BusRejserLibrary.Enums;
using System.Security.Cryptography;
using System.Text;

namespace BusRejserLibrary.Models
{
	public class Booking
	{
		public int BookingId { get; set; }
		public int RejseId { get; private set; }
		public Rejse? Rejse { get; set; }

		public int? UserId { get; private set; }
		public string KundeNavn { get; private set; }
		public string KundeEmail { get; private set; }
		public int AntalPladser { get; private set; }
		public string BookingReference { get; private set; } = "";
		public string? StripeSessionId { get; private set; }
		public string? StripePaymentIntentId { get; private set; }
		public decimal TotalPrice { get; private set; }
		public DateTime CreatedAt { get; private set; }
		public DateTime? PaidAt { get; private set; }
		public BookingStatus Status { get; private set; }

		private Booking(
			int rejseId,
			int? userId,
			string kundeNavn,
			string kundeEmail,
			int antalPladser,
			decimal totalPrice)
		{
			RejseId = rejseId;
			UserId = userId;
			KundeNavn = kundeNavn;
			KundeEmail = kundeEmail;
			AntalPladser = antalPladser;
			TotalPrice = totalPrice;
			CreatedAt = DateTime.UtcNow;
			Status = BookingStatus.Pending;
			BookingReference = GenerateReference();
		}

		public static Booking Create(
			int rejseId,
			int? userId,
			string kundeNavn,
			string kundeEmail,
			int antalPladser,
			decimal totalPrice)
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

			if (totalPrice < 0)
				throw new ArgumentOutOfRangeException(nameof(totalPrice));

			return new Booking(rejseId, userId, kundeNavn, kundeEmail, antalPladser, totalPrice);
		}

		public static Booking Restore(
			int bookingId,
			int rejseId,
			int? userId,
			string bookingReference,
			string kundeNavn,
			string kundeEmail,
			int antalPladser,
			decimal totalPrice,
			BookingStatus status,
			DateTime createdAt,
			DateTime? paidAt,
			string? stripeSessionId,
			string? stripePaymentIntentId)
		{
			var booking = new Booking(rejseId, userId, kundeNavn, kundeEmail, antalPladser, totalPrice);
			booking.BookingId = bookingId;
			booking.BookingReference = bookingReference;
			booking.Status = status;
			booking.CreatedAt = createdAt;
			booking.PaidAt = paidAt;
			booking.StripeSessionId = stripeSessionId;
			booking.StripePaymentIntentId = stripePaymentIntentId;
			return booking;
		}

		public void MarkAsPaid(string? stripeSessionId, string? stripePaymentIntentId)
		{
			if (Status == BookingStatus.Paid)
				return;

			Status = BookingStatus.Paid;
			PaidAt = DateTime.UtcNow;
			StripeSessionId = stripeSessionId;
			StripePaymentIntentId = stripePaymentIntentId;
		}

		public void MarkPaymentFailed(string? stripeSessionId, string? stripePaymentIntentId)
		{
			Status = BookingStatus.PaymentFailed;
			StripeSessionId = stripeSessionId;
			StripePaymentIntentId = stripePaymentIntentId;
		}

		public void Cancel()
		{
			Status = BookingStatus.Cancelled;
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
	}
}