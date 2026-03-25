using BusRejser.Exceptions;
using BusRejserLibrary.Models;
using BusRejserLibrary.Repositories;

namespace BusRejserLibrary.Services
{
	public class RejseService
	{
		private readonly RejseRepository _repo;

		public RejseService(RejseRepository repo)
		{
			_repo = repo;
		}

		public int Create(Rejse rejse) => _repo.Create(rejse);

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

			var updated = _repo.Update(id, rejse);
			if (!updated)
				throw new ConflictException("Rejsen kunne ikke opdateres.");

			return true;
		}
	}
}