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
	public class TravelHistoryRepository
	{
		private readonly BusPlanenDbContext _context;

		public TravelHistoryRepository(BusPlanenDbContext context)
		{
			_context = context;
		}

		public int Create(TravelHistory entity)
		{
			_context.TravelHistories.Add(entity);
			_context.SaveChanges();
			return entity.TravelHistoryId;
		}

		public List<TravelHistory> GetByUserId(int userId)
		{
			return _context.TravelHistories
				.AsNoTracking()
				.Include(x => x.Rejse)
				.Include(x => x.Booking)
				.Where(x => x.UserId == userId)
				.OrderByDescending(x => x.CompletedAt)
				.ToList();
		}

		public bool Exists(int userId, int rejseId)
		{
			return _context.TravelHistories
				.Any(x => x.UserId == userId && x.RejseId == rejseId);
		}

		public List<TravelHistory> GetAll()
		{
			return _context.TravelHistories
				.AsNoTracking()
				.ToList();
		}
	}
}
