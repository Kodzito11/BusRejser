using BusRejser.Exceptions;
using BusRejser.Features.Progression.DTOs;
using BusRejserLibrary.Models;
using BusRejserLibrary.Repositories;

namespace BusRejser.Features.Progression.Services
{
	public class ProgressionTerritoryAdminService
	{
		private readonly ProgressionTerritoryRepository _territoryRepository;

		public ProgressionTerritoryAdminService(
			ProgressionTerritoryRepository territoryRepository)
		{
			_territoryRepository = territoryRepository;
		}

		public List<ProgressionTerritoryAdminResponse> GetAll()
		{
			return _territoryRepository
				.GetAll()
				.Select(ToResponse)
				.ToList();
		}

		public ProgressionTerritoryAdminResponse GetById(int id)
		{
			var territory = _territoryRepository.GetById(id);

			if (territory == null)
				throw new NotFoundException("Progression territory blev ikke fundet.");

			return ToResponse(territory);
		}

		public int Create(CreateProgressionTerritoryRequest request)
		{
			ValidateCreateRequest(request);

			var key = NormalizeKey(request.Key);

			var existingTerritory = _territoryRepository.GetByKey(key);
			if (existingTerritory != null)
				throw new ConflictException("Der findes allerede et progression territory med den key.");

			var territory = new ProgressionTerritory
			{
				Key = key,
				Name = request.Name.Trim(),
				Type = NormalizeType(request.Type),
				IsActive = request.IsActive,
				IsVisible = request.IsVisible,
				IsComingSoon = request.IsComingSoon,
				MasteryTarget = request.MasteryTarget,
				Description = string.IsNullOrWhiteSpace(request.Description)
					? null
					: request.Description.Trim(),
				CreatedAt = DateTime.UtcNow,
				Aliases = request.Aliases
					.Where(x => !string.IsNullOrWhiteSpace(x))
					.Select(x => new ProgressionTerritoryAlias
					{
						Value = NormalizeAlias(x)
					})
					.DistinctBy(x => x.Value)
					.ToList()
			};

			return _territoryRepository.Create(territory);
		}


		public void Update(int id, UpdateProgressionTerritoryRequest request)
		{
			ValidateUpdateRequest(request);

			var key = NormalizeKey(request.Key);

			var territory = _territoryRepository.GetById(id);
			if (territory == null)
				throw new NotFoundException("Progression territory blev ikke fundet.");

			var territoryWithSameKey = _territoryRepository.GetByKey(key);
			if (territoryWithSameKey != null &&
				territoryWithSameKey.ProgressionTerritoryId != id)
			{
				throw new ConflictException("Der findes allerede et progression territory med den key.");
			}

			var updated = new ProgressionTerritory
			{
				ProgressionTerritoryId = id,
				Key = key,
				Name = request.Name.Trim(),
				Type = NormalizeType(request.Type),
				IsActive = request.IsActive,
				IsVisible = request.IsVisible,
				IsComingSoon = request.IsComingSoon,
				MasteryTarget = request.MasteryTarget,
				Description = string.IsNullOrWhiteSpace(request.Description)
					? null
					: request.Description.Trim()
			};

			_territoryRepository.Update(updated);
		}

		public void AddAlias(int territoryId, AddProgressionTerritoryAliasRequest request)
		{
			if (string.IsNullOrWhiteSpace(request.Value))
				throw new ValidationException("Alias kræves.");

			var territory = _territoryRepository.GetById(territoryId);
			if (territory == null)
				throw new NotFoundException("Progression territory blev ikke fundet.");

			_territoryRepository.AddAlias(
				territoryId,
				NormalizeAlias(request.Value)
			);
		}

		public void RemoveAlias(int aliasId)
		{
			_territoryRepository.RemoveAlias(aliasId);
		}

		private static ProgressionTerritoryAdminResponse ToResponse(
			ProgressionTerritory territory)
		{
			return new ProgressionTerritoryAdminResponse
			{
				ProgressionTerritoryId = territory.ProgressionTerritoryId,
				Key = territory.Key,
				Name = territory.Name,
				Type = territory.Type,
				IsActive = territory.IsActive,
				IsVisible = territory.IsVisible,
				IsComingSoon = territory.IsComingSoon,
				MasteryTarget = territory.MasteryTarget,
				Description = territory.Description,
				Aliases = territory.Aliases
					.OrderBy(x => x.Value)
					.Select(x => new ProgressionTerritoryAliasResponse
					{
						ProgressionTerritoryAliasId = x.ProgressionTerritoryAliasId,
						Value = x.Value
					})
					.ToList()
			};
		}

		private static void ValidateCreateRequest(
			CreateProgressionTerritoryRequest request)
		{
			if (string.IsNullOrWhiteSpace(request.Key))
				throw new ValidationException("Key kræves.");

			if (string.IsNullOrWhiteSpace(request.Name))
				throw new ValidationException("Name kræves.");

			if (string.IsNullOrWhiteSpace(request.Type))
				throw new ValidationException("Type kræves.");

			if (request.MasteryTarget <= 0)
				throw new ValidationException("MasteryTarget skal være større end 0.");
		}

		private static void ValidateUpdateRequest(
			UpdateProgressionTerritoryRequest request)
		{
			if (string.IsNullOrWhiteSpace(request.Key))
				throw new ValidationException("Key kræves.");

			if (string.IsNullOrWhiteSpace(request.Name))
				throw new ValidationException("Name kræves.");

			if (string.IsNullOrWhiteSpace(request.Type))
				throw new ValidationException("Type kræves.");

			if (request.MasteryTarget <= 0)
				throw new ValidationException("MasteryTarget skal være større end 0.");
		}

		private static string NormalizeKey(string value)
		{
			return value.Trim().ToLowerInvariant();
		}

		private static string NormalizeType(string value)
		{
			return value.Trim().ToLowerInvariant();
		}

		private static string NormalizeAlias(string value)
		{
			return value.Trim().ToLowerInvariant();
		}
	}
}