using BusRejser.Exceptions;
using BusRejserLibrary.Models;
using BusRejserLibrary.Repositories;

namespace BusRejser.Features.Rejser.Services
{
	public class RejseService
	{
		private readonly RejseRepository _repo;
		private readonly ProgressionTerritoryRepository _territoryRepository;

		public RejseService(
			RejseRepository repo,
			ProgressionTerritoryRepository territoryRepository)
		{
			_repo = repo;
			_territoryRepository = territoryRepository;
		}

		public int Create(Rejse rejse)
		{
			ValidateProgressionTerritory(rejse.ProgressionTerritoryId);

			return _repo.Create(rejse);
		}

		public Rejse? GetById(int id) => _repo.GetById(id);

		public List<Rejse> GetAll() => _repo.GetAll();

		public bool Delete(int id)
		{
			var existing = _repo.GetById(id);
			if (existing == null)
				throw new NotFoundException("Rejse blev ikke fundet.");

			if (existing.BookedSeats > 0)
				throw new ConflictException("Rejsen kan ikke slettes, fordi der allerede findes bookinger.");

			var deleted = _repo.Delete(id);
			if (!deleted)
				throw new ConflictException("Rejsen kunne ikke slettes.");

			return true;
		}

		public bool Update(int id, Rejse rejse)
		{
			var existing = _repo.GetById(id);
			if (existing == null)
				throw new NotFoundException("Rejse blev ikke fundet.");

			if (rejse.MaxSeats < existing.BookedSeats)
				throw new ValidationException("MaxSeats kan ikke være mindre end allerede bookede pladser.");

			ValidateProgressionTerritory(rejse.ProgressionTerritoryId);

			var updated = _repo.Update(id, rejse);
			if (!updated)
				throw new ConflictException("Rejsen kunne ikke opdateres.");

			return true;
		}

		private void ValidateProgressionTerritory(int? progressionTerritoryId)
		{
			if (!progressionTerritoryId.HasValue)
				return;

			var territory = _territoryRepository.GetById(progressionTerritoryId.Value);
			if (territory == null)
				throw new ValidationException("Progression territory blev ikke fundet.");
		}
	}
}