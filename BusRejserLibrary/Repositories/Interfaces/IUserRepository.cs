using BusRejserLibrary.Models;

namespace BusRejserLibrary.Repositories
{
	public interface IUserRepository
	{
		int Create(User user);
		User? GetById(int id);
		User? GetByEmail(string email);
		List<User> GetAll();
		bool Update(User user);
		bool Delete(int id);
	}
}