using BusRejser.DTOs;
using BusRejserLibrary.Models;

namespace BusRejser.Mappers
{
	public static class RejseMapper
	{
		public static RejseResponse ToResponse(Rejse r)
		{
			return new RejseResponse
			{
				RejseId = r.RejseId,
				Title = r.Title,
				Destination = r.Destination,
				StartAt = r.StartAt,
				EndAt = r.EndAt,
				Price = r.Price,
				MaxSeats = r.MaxSeats,
				BookedSeats = r.BookedSeats,
				BusId = r.BusId,
				ShortDescription = r.ShortDescription,
				Description = r.Description,
				ImageUrl = r.ImageUrl,
				IsFeatured = r.IsFeatured,
				IsPublished = r.IsPublished
			};
		}
	}
}