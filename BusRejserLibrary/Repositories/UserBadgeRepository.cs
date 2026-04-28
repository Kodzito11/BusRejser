using BusRejserLibrary.Database;
using BusRejserLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace BusRejserLibrary.Repositories
{
	public class UserBadgeRepository
	{
		private readonly BusPlanenDbContext _context;

		public UserBadgeRepository(BusPlanenDbContext context)
		{
			_context = context;
		}

		public int Create(UserBadge entity)
		{
			_context.UserBadges.Add(entity);
			_context.SaveChanges();
			return entity.UserBadgeId;
		}

		public List<UserBadge> GetByUserId(int userId)
		{
			return _context.UserBadges
				.AsNoTracking()
				.Where(x => x.UserId == userId)
				.ToList();
		}

		public List<UserBadge> GetByUserIdWithBadge(int userId)
		{
			return _context.UserBadges
				.AsNoTracking()
				.Include(x => x.Badge)
				.Where(x => x.UserId == userId)
				.ToList();
		}

		public bool Exists(int userId, int badgeId)
		{
			return _context.UserBadges
				.Any(x => x.UserId == userId && x.BadgeId == badgeId);
		}
	}
}