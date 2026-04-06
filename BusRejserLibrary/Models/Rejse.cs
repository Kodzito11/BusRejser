using System.ComponentModel.DataAnnotations;

namespace BusRejserLibrary.Models
{
	public class Rejse
	{
		public int RejseId { get; set; }
		public string Title { get; set; }
		public string Destination { get; set; }
		public DateTime StartAt { get; set; }
		public DateTime EndAt { get; set; }
		public decimal Price { get; set; }
		public int MaxSeats { get; set; }
		public int? BusId { get; set; }
		public int BookedSeats { get; set; }

		[ConcurrencyCheck]
		public long Version { get; set; }

		public string? ShortDescription { get; set; }
		public string? Description { get; set; }
		public string? ImageUrl { get; set; }
		public bool IsFeatured { get; set; }
		public bool IsPublished { get; set; }

		private Rejse(
			string title,
			string destination,
			DateTime startAt,
			DateTime endAt,
			decimal price,
			int maxSeats,
			int? busId,
			string? shortDescription,
			string? description,
			string? imageUrl,
			bool isFeatured,
			bool isPublished)
		{
			Title = title;
			Destination = destination;
			StartAt = startAt;
			EndAt = endAt;
			Price = price;
			MaxSeats = maxSeats;
			BusId = busId;
			BookedSeats = 0;

			ShortDescription = shortDescription;
			Description = description;
			ImageUrl = imageUrl;
			IsFeatured = isFeatured;
			IsPublished = isPublished;
		}

		public static Rejse Create(
			string title,
			string destination,
			DateTime startAt,
			DateTime endAt,
			decimal price,
			int maxSeats,
			int? busId,
			string? shortDescription,
			string? description,
			string? imageUrl,
			bool isFeatured,
			bool isPublished)
		{
			if (string.IsNullOrWhiteSpace(title))
				throw new ArgumentException("Title kræves.");

			if (string.IsNullOrWhiteSpace(destination))
				throw new ArgumentException("Destination kræves.");

			if (endAt < startAt)
				throw new ArgumentException("EndAt kan ikke være før StartAt.");

			if (price < 0)
				throw new ArgumentOutOfRangeException(nameof(price));

			if (maxSeats <= 0)
				throw new ArgumentOutOfRangeException(nameof(maxSeats), "MaxSeats skal være større end 0.");

			if (!string.IsNullOrWhiteSpace(shortDescription) && shortDescription.Length > 300)
				throw new ArgumentException("ShortDescription må max være 300 tegn.");

			return new Rejse(
				title.Trim(),
				destination.Trim(),
				startAt,
				endAt,
				price,
				maxSeats,
				busId,
				string.IsNullOrWhiteSpace(shortDescription) ? null : shortDescription.Trim(),
				string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
				string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim(),
				isFeatured,
				isPublished
			);
		}
	}
}