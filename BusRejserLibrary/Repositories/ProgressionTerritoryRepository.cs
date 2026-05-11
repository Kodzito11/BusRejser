using BusRejserLibrary.Database;
using BusRejserLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace BusRejserLibrary.Repositories
{
	public class ProgressionTerritoryRepository
	{
		private readonly BusPlanenDbContext _context;

		public ProgressionTerritoryRepository(BusPlanenDbContext context)
		{
			_context = context;
		}

		public List<ProgressionTerritory> GetAll()
		{
			return _context.ProgressionTerritories
				.AsNoTracking()
				.Include(x => x.Aliases)
				.OrderBy(x => x.Name)
				.ToList();
		}

		public List<ProgressionTerritory> GetVisibleWithAliases()
		{
			return _context.ProgressionTerritories
				.AsNoTracking()
				.Include(x => x.Aliases)
				.Where(x => x.IsVisible)
				.OrderBy(x => x.Name)
				.ToList();
		}

		public List<ProgressionTerritory> GetActiveWithAliases()
		{
			return _context.ProgressionTerritories
				.AsNoTracking()
				.Include(x => x.Aliases)
				.Where(x => x.IsActive)
				.OrderBy(x => x.Name)
				.ToList();
		}

		public ProgressionTerritory? GetById(int id)
		{
			return _context.ProgressionTerritories
				.AsNoTracking()
				.Include(x => x.Aliases)
				.FirstOrDefault(x => x.ProgressionTerritoryId == id);
		}

		public ProgressionTerritory? GetByKey(string key)
		{
			var normalizedKey = key.Trim().ToLowerInvariant();

			return _context.ProgressionTerritories
				.AsNoTracking()
				.Include(x => x.Aliases)
				.FirstOrDefault(x => x.Key.ToLower() == normalizedKey);
		}

		public int Create(ProgressionTerritory territory)
		{
			_context.ProgressionTerritories.Add(territory);
			_context.SaveChanges();

			return territory.ProgressionTerritoryId;
		}

		public void Update(ProgressionTerritory territory)
		{
			var existing = _context.ProgressionTerritories
				.FirstOrDefault(x => x.ProgressionTerritoryId == territory.ProgressionTerritoryId);

			if (existing == null)
				return;

			existing.Key = territory.Key;
			existing.Name = territory.Name;
			existing.Type = territory.Type;
			existing.IsActive = territory.IsActive;
			existing.IsVisible = territory.IsVisible;
			existing.IsComingSoon = territory.IsComingSoon;
			existing.MasteryTarget = territory.MasteryTarget;
			existing.Description = territory.Description;
			existing.UpdatedAt = DateTime.UtcNow;

			_context.SaveChanges();
		}

		public void AddAlias(int territoryId, string value)
		{
			var normalizedValue = value.Trim().ToLowerInvariant();

			var exists = _context.ProgressionTerritoryAliases
				.Any(x =>
					x.ProgressionTerritoryId == territoryId &&
					x.Value.ToLower() == normalizedValue);

			if (exists)
				return;

			var alias = new ProgressionTerritoryAlias
			{
				ProgressionTerritoryId = territoryId,
				Value = normalizedValue
			};

			_context.ProgressionTerritoryAliases.Add(alias);
			_context.SaveChanges();
		}

		public void RemoveAlias(int aliasId)
		{
			var alias = _context.ProgressionTerritoryAliases
				.FirstOrDefault(x => x.ProgressionTerritoryAliasId == aliasId);

			if (alias == null)
				return;

			_context.ProgressionTerritoryAliases.Remove(alias);
			_context.SaveChanges();
		}
	}
}