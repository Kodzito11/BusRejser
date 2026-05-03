using BusRejserLibrary.Database;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using BusRejserLibrary.Models;

namespace BusRejserLibrary.Repositories
{
	public class BusFacilitetRepository
	{
		private readonly BusPlanenDbContext _context;

		public BusFacilitetRepository(BusPlanenDbContext context)
		{
			_context = context;
		}

		public List<int> GetFacilitetIdsForBus(int busId)
		{
			var bus = _context.Buses
				.AsNoTracking()
				.Include(x => x.Faciliteter)
				.FirstOrDefault(x => x.BusId == busId);

			if (bus == null)
				return new List<int>();

			return bus.Faciliteter.Select(x => x.Id).ToList();
		}

		public bool Add(int busId, int facilitetId)
		{
			var bus = _context.Buses
				.Include(x => x.Faciliteter)
				.FirstOrDefault(x => x.BusId == busId);

			var facilitet = _context.Faciliteter
				.FirstOrDefault(x => x.Id == facilitetId);

			if (bus == null || facilitet == null)
				return false;

			if (bus.Faciliteter.Any(x => x.Id == facilitetId))
				return true;

			bus.Faciliteter.Add(facilitet);
			_context.SaveChanges();

			return true;
		}

		public bool Remove(int busId, int facilitetId)
		{
			var bus = _context.Buses
				.Include(x => x.Faciliteter)
				.FirstOrDefault(x => x.BusId == busId);

			if (bus == null)
				return false;

			var facilitet = bus.Faciliteter.FirstOrDefault(x => x.Id == facilitetId);
			if (facilitet == null)
				return false;

			bus.Faciliteter.Remove(facilitet);
			_context.SaveChanges();

			return true;
		}
	}
}