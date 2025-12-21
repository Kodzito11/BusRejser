using System;
using System.Collections.Generic;
using BusRejserLibrary.Models;
using BusRejserLibrary.Repositories;

namespace BusRejserLibrary.Services
{
	public class BusService
	{
		private readonly BusRepository _busRepository;
		private readonly FacilitetRepository _facilitetRepository;
		private readonly BusFacilitetRepository _busFacilitetRepository;

		public BusService(
			BusRepository busRepository,
			FacilitetRepository facilitetRepository,
			BusFacilitetRepository busFacilitetRepository)
		{
			_busRepository = busRepository;
			_facilitetRepository = facilitetRepository;
			_busFacilitetRepository = busFacilitetRepository;
		}

		public int Create(Bus bus) => _busRepository.Create(bus);

		public Bus? GetById(int id) => _busRepository.GetById(id);

		public List<Bus> GetAll() => _busRepository.GetAll();

		public bool Update(Bus bus) => _busRepository.Update(bus);

		public bool Delete(int id) => _busRepository.Delete(id);

		public List<Facilitet> GetFaciliteterForBus(int busId)
		{
			var ids = _busFacilitetRepository.GetFacilitetIdsForBus(busId);
			var list = new List<Facilitet>();

			foreach (var fid in ids)
			{
				var f = _facilitetRepository.GetById(fid);
				if (f != null) list.Add(f);
			}

			return list;
		}

		public bool AddFacilitet(int busId, int facilitetId)
		{
			// quick sanity checks
			if (_busRepository.GetById(busId) == null) return false;
			if (_facilitetRepository.GetById(facilitetId) == null) return false;

			return _busFacilitetRepository.Add(busId, facilitetId);
		}

		public bool RemoveFacilitet(int busId, int facilitetId)
		{
			return _busFacilitetRepository.Remove(busId, facilitetId);
		}
	}
}
