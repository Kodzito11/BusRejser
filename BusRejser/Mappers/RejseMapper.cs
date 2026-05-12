using BusRejser.Features.Rejser.DTOs;
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
				Country = r.Country,
				City = r.City,
				Region = r.Region,
				Municipality = r.Municipality,
				Latitude = r.Latitude,
				Longitude = r.Longitude,
				StartAt = r.StartAt,
				EndAt = r.EndAt,
				Price = r.Price,
				MaxSeats = r.MaxSeats,
				BookedSeats = r.BookedSeats,
				BusId = r.BusId,
				ProgressionTerritoryId = r.ProgressionTerritoryId,
				ProgressionTerritoryName = r.ProgressionTerritory?.Name,
				ProgressionTerritoryKey = r.ProgressionTerritory?.Key,
				ShortDescription = r.ShortDescription,
				Description = r.Description,
				ImageUrl = r.ImageUrl,
				IsFeatured = r.IsFeatured,
				IsPublished = r.IsPublished
			};
		}
	}
}