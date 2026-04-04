using BusRejserLibrary.Database;
using BusRejserLibrary.Models;

namespace BusRejserLibrary.Repositories
{
	public class FacilitetRepository
	{
		private readonly BusPlanenDbContext _context;

		public FacilitetRepository(BusPlanenDbContext context)
		{
			_context = context;
		}

		public int Create(Facilitet facilitet)
		{
			if (facilitet == null)
				throw new ArgumentNullException(nameof(facilitet));

			_context.Faciliteter.Add(facilitet);
			_context.SaveChanges();

			return facilitet.Id;
		}

		public Facilitet? GetById(int id)
		{
			return _context.Faciliteter.FirstOrDefault(x => x.Id == id);
		}

		public List<Facilitet> GetAll()
		{
			return _context.Faciliteter.ToList();
		}

		public bool Update(Facilitet facilitet)
		{
			if (facilitet == null)
				throw new ArgumentNullException(nameof(facilitet));

			var existing = _context.Faciliteter.FirstOrDefault(x => x.Id == facilitet.Id);
			if (existing == null)
				return false;

			existing.Update(
				facilitet.Name,
				facilitet.Description,
				facilitet.ExtraPrice,
				facilitet.IsActive,
				facilitet.Type
			);

			_context.SaveChanges();
			return true;
		}

		public bool Delete(int id)
		{
			var facilitet = _context.Faciliteter.FirstOrDefault(x => x.Id == id);
			if (facilitet == null)
				return false;

			_context.Faciliteter.Remove(facilitet);
			_context.SaveChanges();
			return true;
		}
	}
}