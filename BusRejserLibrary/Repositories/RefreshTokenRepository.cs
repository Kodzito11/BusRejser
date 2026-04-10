using BusRejserLibrary.Database;
using BusRejserLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace BusRejserLibrary.Repositories
{
	public class RefreshTokenRepository
	{
		private readonly BusPlanenDbContext _context;

		public RefreshTokenRepository(BusPlanenDbContext context)
		{
			_context = context;
		}

		public void Create(RefreshToken token)
		{
			if (token == null)
				throw new ArgumentNullException(nameof(token));

			_context.RefreshTokens.Add(token);
			_context.SaveChanges();
		}

		public RefreshToken? GetByTokenHash(string tokenHash)
		{
			return _context.RefreshTokens
				.FirstOrDefault(x => x.TokenHash == tokenHash);
		}

		public void Rotate(RefreshToken currentToken, RefreshToken newToken)
		{
			if (currentToken == null)
				throw new ArgumentNullException(nameof(currentToken));

			if (newToken == null)
				throw new ArgumentNullException(nameof(newToken));

			currentToken.MarkUsed();
			currentToken.Revoke(newToken.TokenHash);

			_context.RefreshTokens.Add(newToken);
			_context.SaveChanges();
		}

		public void Revoke(RefreshToken token)
		{
			if (token == null)
				throw new ArgumentNullException(nameof(token));

			token.Revoke();
			_context.SaveChanges();
		}

		public void RevokeAllForUser(int userId)
		{
			var tokens = _context.RefreshTokens
				.Where(x => x.UserId == userId && x.RevokedAt == null)
				.ToList();

			if (tokens.Count == 0)
				return;

			foreach (var token in tokens)
			{
				token.Revoke();
			}

			_context.SaveChanges();
		}
	}
}
