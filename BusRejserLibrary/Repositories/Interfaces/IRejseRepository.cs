using BusRejserLibrary.Models;

namespace BusRejserLibrary.Repositories
{
	public interface IRejseRepository
	{
		int Create(Rejse rejse);
		Rejse? GetById(int id);
		List<Rejse> GetAll();
		bool Delete(int id);
		bool Update(int id, Rejse rejse);
		bool TryReserveSeats(int rejseId, int antalPladser);
		bool ReleaseSeats(int rejseId, int antalPladser);
	}
}