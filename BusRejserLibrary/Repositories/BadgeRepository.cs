using BusRejserLibrary.Database;
using BusRejserLibrary.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusRejserLibrary.Repositories
{
	public class BadgeRepository
	{
		private readonly BusPlanenDbContext _context;

		public BadgeRepository(BusPlanenDbContext context)
		{
			_context = context;
		}

		public List<Badge> GetAllActive()
		{
			return _context.Badges
				.AsNoTracking()
				.Where(x => x.IsActive)
				.ToList();
		}

		public List<Badge> GetAll()
		{
			return _context.Badges
				.AsNoTracking()
				.ToList();
		}

		public int Create(Badge badge)
		{
			_context.Badges.Add(badge);
			_context.SaveChanges();
			return badge.BadgeId;
		}

		public Badge? GetById(int id)
		{
			return _context.Badges
				.AsNoTracking()
				.FirstOrDefault(x => x.BadgeId == id);
		}
	}
}