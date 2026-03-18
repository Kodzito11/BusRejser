using System.Collections.Generic;
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
			if (existing == null) return false;

			if (existing.BookedSeats > 0)
				throw new Exception("Rejsen kan ikke slettes, fordi der allerede findes bookinger.");

			return _repo.Delete(id);
		}

		public bool Update(int id, Rejse rejse)
		{
			var existing = _repo.GetById(id);
			if (existing == null) return false;

			if (rejse.MaxSeats < existing.BookedSeats)
				throw new Exception("MaxSeats kan ikke være mindre end allerede bookede pladser.");

			return _repo.Update(id, rejse);
		}

	}
}