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
		public bool Delete(int id) => _repo.Delete(id);
	}
}