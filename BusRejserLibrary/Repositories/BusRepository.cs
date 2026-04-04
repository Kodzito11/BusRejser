using BusRejserLibrary.Database;
using BusRejserLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace BusRejserLibrary.Repositories
{
	public class BusRepository
	{
		private readonly BusPlanenDbContext _context;

		public BusRepository(BusPlanenDbContext context)
		{
			_context = context;
		}

		public int Create(Bus bus)
		{
			if (bus == null)
				throw new ArgumentNullException(nameof(bus));

			_context.Buses.Add(bus);
			_context.SaveChanges();

			return bus.busId;
		}

		public Bus? GetById(int id)
		{
			return _context.Buses
				.AsNoTracking()
				.Include(x => x.Faceliteter)
				.FirstOrDefault(x => x.busId == id);
		}

		public List<Bus> GetAll()
		{
			return _context.Buses
				.AsNoTracking()
				.Include(x => x.Faceliteter)
				.ToList();
		}

		public bool Update(Bus bus)
		{
			if (bus == null)
				throw new ArgumentNullException(nameof(bus));

			var existing = _context.Buses
				.FirstOrDefault(x => x.busId == bus.busId);

			if (existing == null)
				return false;

			existing.Registreringnummer = bus.Registreringnummer;
			existing.Model = bus.Model;
			existing.Busselskab = bus.Busselskab;
			existing.Status = bus.Status;
			existing.Type = bus.Type;
			existing.Kapasitet = bus.Kapasitet;
			existing.ImageUrl = bus.ImageUrl;

			_context.SaveChanges();
			return true;
		}

		public bool Delete(int id)
		{
			var bus = _context.Buses.FirstOrDefault(x => x.busId == id);
			if (bus == null)
				return false;

			_context.Buses.Remove(bus);
			_context.SaveChanges();
			return true;
		}
	}
}