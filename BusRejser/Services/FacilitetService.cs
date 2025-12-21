using System.Collections.Generic;
using BusRejserLibrary.Models;
using BusRejserLibrary.Repositories;

namespace BusRejserLibrary.Services
{
	public class FacilitetService
	{
		private readonly FacilitetRepository _facilitetRepository;

		public FacilitetService(FacilitetRepository facilitetRepository)
		{
			_facilitetRepository = facilitetRepository;
		}

		public int Create(Facilitet facilitet) => _facilitetRepository.Create(facilitet);

		public Facilitet? GetById(int id) => _facilitetRepository.GetById(id);

		public List<Facilitet> GetAll() => _facilitetRepository.GetAll();

		public bool Delete(int id) => _facilitetRepository.Delete(id);
	}
}
