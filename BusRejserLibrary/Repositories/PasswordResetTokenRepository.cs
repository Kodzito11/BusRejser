using BusRejserLibrary.Database;
using BusRejserLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace BusRejserLibrary.Repositories
{
	public class PasswordResetTokenRepository
	{
		private readonly BusPlanenDbContext _context;

		public PasswordResetTokenRepository(BusPlanenDbContext context)
		{
			_context = context;
		}

		public void Create(PasswordResetToken token)
		{
			if (token == null)
				throw new ArgumentNullException(nameof(token));

			_context.PasswordResetTokens.Add(token);
			_context.SaveChanges();
		}

		public PasswordResetToken? GetActiveByHash(string hash)
		{
			return _context.PasswordResetTokens
				.FirstOrDefault(x => x.TokenHash == hash && x.UsedAt == null);
		}

		public void MarkAsUsed(int id)
		{
			var token = _context.PasswordResetTokens
				.FirstOrDefault(x => x.Id == id);

			if (token == null)
				return;

			token.UsedAt = DateTime.UtcNow;
			_context.SaveChanges();
		}

		public void InvalidateAllForUser(int userId)
		{
			var tokens = _context.PasswordResetTokens
				.Where(x => x.UserId == userId && x.UsedAt == null)
				.ToList();

			if (tokens.Count == 0)
				return;

			foreach (var token in tokens)
			{
				token.UsedAt = DateTime.UtcNow;
			}

			_context.SaveChanges();
		}
	}
}