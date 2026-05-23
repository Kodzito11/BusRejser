using BusRejserLibrary.Database;
using BusRejserLibrary.Models;

namespace BusRejserLibrary.Repositories
{
	public class EmailVerificationTokenRepository
	{
		private readonly BusPlanenDbContext _context;

		public EmailVerificationTokenRepository(BusPlanenDbContext context)
		{
			_context = context;
		}

		public int Create(EmailVerificationToken token)
		{
			_context.EmailVerificationTokens.Add(token);
			_context.SaveChanges();

			return token.EmailVerificationTokenId;
		}

		public EmailVerificationToken? GetActiveByHash(string tokenHash)
		{
			return _context.EmailVerificationTokens
				.FirstOrDefault(token =>
					token.TokenHash == tokenHash &&
					token.UsedAt == null &&
					token.ExpiresAt > DateTime.UtcNow);
		}

		public void MarkAsUsed(int emailVerificationTokenId)
		{
			var token = _context.EmailVerificationTokens
				.FirstOrDefault(token => token.EmailVerificationTokenId == emailVerificationTokenId);

			if (token == null)
				return;

			token.UsedAt = DateTime.UtcNow;
			_context.SaveChanges();
		}

		public void InvalidateAllForUser(int userId)
		{
			var activeTokens = _context.EmailVerificationTokens
				.Where(token =>
					token.UserId == userId &&
					token.UsedAt == null &&
					token.ExpiresAt > DateTime.UtcNow)
				.ToList();

			foreach (var token in activeTokens)
			{
				token.UsedAt = DateTime.UtcNow;
			}

			_context.SaveChanges();
		}
	}
}