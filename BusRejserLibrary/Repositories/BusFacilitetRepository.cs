using BusRejserLibrary.Database;
using Microsoft.EntityFrameworkCore;

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
				.Include(x => x.Faceliteter)
				.FirstOrDefault(x => x.busId == busId);

			if (bus == null)
				return new List<int>();

			return bus.Faceliteter.Select(x => x.Id).ToList();
		}

		public bool Add(int busId, int facilitetId)
		{
			var bus = _context.Buses
				.Include(x => x.Faceliteter)
				.FirstOrDefault(x => x.busId == busId);

			var facilitet = _context.Faciliteter
				.FirstOrDefault(x => x.Id == facilitetId);

			if (bus == null || facilitet == null)
				return false;

			if (bus.Faceliteter.Any(x => x.Id == facilitetId))
				return true;

			bus.Faceliteter.Add(facilitet);
			_context.SaveChanges();

			return true;
		}

		public bool Remove(int busId, int facilitetId)
		{
			var bus = _context.Buses
				.Include(x => x.Faceliteter)
				.FirstOrDefault(x => x.busId == busId);

			if (bus == null)
				return false;

			var facilitet = bus.Faceliteter.FirstOrDefault(x => x.Id == facilitetId);
			if (facilitet == null)
				return false;

			bus.Faceliteter.Remove(facilitet);
			_context.SaveChanges();

			return true;
		}
	}
}