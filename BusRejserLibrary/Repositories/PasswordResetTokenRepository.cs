using BusRejserLibrary.Models;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusRejserLibrary.Repositories
{
	public class PasswordResetTokenRepository
	{
		private readonly string _connectionString;

		public PasswordResetTokenRepository(string connectionString)
		{
			_connectionString = connectionString;
		}

		public void Create(PasswordResetToken token)
		{
			using var conn = new MySqlConnection(_connectionString);
			conn.Open();

			var cmd = conn.CreateCommand();
			cmd.CommandText = @"
            INSERT INTO PasswordResetTokens 
            (UserId, TokenHash, ExpiresAt, CreatedAt)
            VALUES (@userId, @hash, @expiresAt, @createdAt)
        ";

			cmd.Parameters.AddWithValue("@userId", token.UserId);
			cmd.Parameters.AddWithValue("@hash", token.TokenHash);
			cmd.Parameters.AddWithValue("@expiresAt", token.ExpiresAt);
			cmd.Parameters.AddWithValue("@createdAt", token.CreatedAt);

			cmd.ExecuteNonQuery();
		}

		public PasswordResetToken? GetActiveByHash(string hash)
		{
			using var conn = new MySqlConnection(_connectionString);
			conn.Open();

			var cmd = conn.CreateCommand();
			cmd.CommandText = @"
            SELECT * FROM PasswordResetTokens
            WHERE TokenHash = @hash
              AND UsedAt IS NULL
            LIMIT 1
        ";

			cmd.Parameters.AddWithValue("@hash", hash);

			using var reader = cmd.ExecuteReader();
			if (!reader.Read()) return null;

			var usedAtIndex = reader.GetOrdinal("UsedAt");

			return new PasswordResetToken
			{
				Id = reader.GetInt32("Id"),
				UserId = reader.GetInt32("UserId"),
				TokenHash = reader.GetString("TokenHash"),
				ExpiresAt = reader.GetDateTime("ExpiresAt"),
				UsedAt = reader.IsDBNull(usedAtIndex) ? null : reader.GetDateTime(usedAtIndex),
				CreatedAt = reader.GetDateTime("CreatedAt")
			};
		}

		public void MarkAsUsed(int id)
		{
			using var conn = new MySqlConnection(_connectionString);
			conn.Open();

			var cmd = conn.CreateCommand();
			cmd.CommandText = @"
            UPDATE PasswordResetTokens
            SET UsedAt = @usedAt
            WHERE Id = @id
        ";

			cmd.Parameters.AddWithValue("@usedAt", DateTime.UtcNow);
			cmd.Parameters.AddWithValue("@id", id);

			cmd.ExecuteNonQuery();
		}

		public void InvalidateAllForUser(int userId)
		{
			using var conn = new MySqlConnection(_connectionString);
			conn.Open();

			var cmd = conn.CreateCommand();
			cmd.CommandText = @"
            UPDATE PasswordResetTokens
            SET UsedAt = @usedAt
            WHERE UserId = @userId
              AND UsedAt IS NULL
        ";

			cmd.Parameters.AddWithValue("@usedAt", DateTime.UtcNow);
			cmd.Parameters.AddWithValue("@userId", userId);

			cmd.ExecuteNonQuery();
		}
	}
}
