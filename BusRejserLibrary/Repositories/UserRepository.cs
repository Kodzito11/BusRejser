using BusRejserLibrary.Database;
using BusRejserLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace BusRejserLibrary.Repositories
{
	public class UserRepository : IUserRepository
	{
		private readonly BusPlanenDbContext _context;

		public UserRepository(BusPlanenDbContext context)
		{
			_context = context;
		}

		public int Create(User user)
		{
			if (user == null)
				throw new ArgumentNullException(nameof(user));

			_context.Users.Add(user);
			_context.SaveChanges();

			return user.UserId;
		}

		public User? GetById(int id)
		{
			return _context.Users
				.AsNoTracking()
				.FirstOrDefault(x => x.UserId == id);
		}

		public User? GetByEmail(string email)
		{
			return _context.Users
				.AsNoTracking()
				.FirstOrDefault(x => x.Email == email);
		}

		public User? GetByUsername(string username)
		{
			return _context.Users
				.AsNoTracking()
				.FirstOrDefault(x => x.Username == username);
		}

		public List<User> GetAll()
		{
			return _context.Users
				.AsNoTracking()
				.OrderBy(x => x.UserId)
				.ToList();
		}

		public bool Update(User user)
		{
			if (user == null)
				throw new ArgumentNullException(nameof(user));

			var existing = _context.Users.FirstOrDefault(x => x.UserId == user.UserId);
			if (existing == null)
				return false;

			existing.Username = user.Username;
			existing.FullName = user.FullName;
			existing.FirstName = user.FirstName;
			existing.LastName = user.LastName;
			existing.Email = user.Email;
			existing.PhoneNumber = user.PhoneNumber;
			existing.PasswordHash = user.PasswordHash;
			existing.IsActive = user.IsActive;
			existing.EmailConfirmed = user.EmailConfirmed;
			existing.Role = user.Role;
			existing.LastLoginAt = user.LastLoginAt;
			existing.UpdatedAt = DateTime.UtcNow;
			_context.SaveChanges();
			return true;
		}

		public bool Delete(int id)
		{
			var user = _context.Users.FirstOrDefault(x => x.UserId == id);
			if (user == null)
				return false;

			_context.Users.Remove(user);
			_context.SaveChanges();
			return true;
		}
	}
}
