using System;
using System.Collections.Generic;
using MySqlConnector;
using BusRejserLibrary.Models;

namespace BusRejserLibrary.Repositories
{
	public class UserRepository : IUserRepository
	{
		private readonly string _connectionString;

		public UserRepository(string connectionString)
		{
			_connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
		}

		public int Create(User user)
		{
			if (user == null) throw new ArgumentNullException(nameof(user));

			const string sql = @"
			INSERT INTO users (Username, Email, PasswordHash, Role, CreatedAt)
			VALUES (@username, @email, @passwordHash, @role, @createdAt);
			SELECT LAST_INSERT_ID();";

			using var conn = new MySqlConnection(_connectionString);
			conn.Open();

			using var cmd = new MySqlCommand(sql, conn);
			cmd.Parameters.AddWithValue("@username", user.Username);
			cmd.Parameters.AddWithValue("@email", user.Email);
			cmd.Parameters.AddWithValue("@passwordHash", user.PasswordHash);
			cmd.Parameters.AddWithValue("@role", (int)user.Role);
			cmd.Parameters.AddWithValue("@createdAt", user.CreatedAt);

			var idObj = cmd.ExecuteScalar();
			var newId = Convert.ToInt32(idObj);

			user.Id = newId;
			return newId;
		}

		public User? GetById(int id)
		{
			const string sql = @"
			SELECT Id, Username, Email, PasswordHash, Role, CreatedAt
			FROM users
			WHERE Id = @id
			LIMIT 1;";

			using var conn = new MySqlConnection(_connectionString);
			conn.Open();

			using var cmd = new MySqlCommand(sql, conn);
			cmd.Parameters.AddWithValue("@id", id);

			using var reader = cmd.ExecuteReader();
			if (!reader.Read()) return null;

			return MapUser(reader);
		}

		public User? GetByEmail(string email)
		{
			const string sql = @"
			SELECT Id, Username, Email, PasswordHash, Role, CreatedAt
			FROM users
			WHERE Email = @email
			LIMIT 1;";

			using var conn = new MySqlConnection(_connectionString);
			conn.Open();

			using var cmd = new MySqlCommand(sql, conn);
			cmd.Parameters.AddWithValue("@email", email);

			using var reader = cmd.ExecuteReader();
			if (!reader.Read()) return null;

			return MapUser(reader);
		}

		public User? GetByUsername(string username)
		{
			const string sql = @"
			SELECT Id, Username, Email, PasswordHash, Role, CreatedAt
			FROM users
			WHERE Username = @username
			LIMIT 1;";

			using var conn = new MySqlConnection(_connectionString);
			conn.Open();

			using var cmd = new MySqlCommand(sql, conn);
			cmd.Parameters.AddWithValue("@username", username);

			using var reader = cmd.ExecuteReader();
			if (!reader.Read()) return null;

			return MapUser(reader);
		}

		public List<User> GetAll()
		{
			const string sql = @"
			SELECT Id, Username, Email, PasswordHash, Role, CreatedAt
			FROM users;";

			var list = new List<User>();

			using var conn = new MySqlConnection(_connectionString);
			conn.Open();

			using var cmd = new MySqlCommand(sql, conn);
			using var reader = cmd.ExecuteReader();

			while (reader.Read())
				list.Add(MapUser(reader));

			return list;
		}

		public bool Update(User user)
		{
			if (user == null) throw new ArgumentNullException(nameof(user));

			const string sql = @"
			UPDATE users
			SET Username = @username,
				Email = @email,
				PasswordHash = @passwordHash,
				Role = @role
			WHERE Id = @id;";

			using var conn = new MySqlConnection(_connectionString);
			conn.Open();

			using var cmd = new MySqlCommand(sql, conn);
			cmd.Parameters.AddWithValue("@id", user.Id);
			cmd.Parameters.AddWithValue("@username", user.Username);
			cmd.Parameters.AddWithValue("@email", user.Email);
			cmd.Parameters.AddWithValue("@passwordHash", user.PasswordHash);
			cmd.Parameters.AddWithValue("@role", (int)user.Role);

			return cmd.ExecuteNonQuery() > 0;
		}

		public bool Delete(int id)
		{
			const string sql = @"DELETE FROM users WHERE Id = @id;";

			using var conn = new MySqlConnection(_connectionString);
			conn.Open();

			using var cmd = new MySqlCommand(sql, conn);
			cmd.Parameters.AddWithValue("@id", id);

			return cmd.ExecuteNonQuery() > 0;
		}

		private static User MapUser(MySqlDataReader reader)
		{
			return new User
			{
				Id = reader.GetInt32("Id"),
				Username = reader.GetString("Username"),
				Email = reader.GetString("Email"),
				PasswordHash = reader.GetString("PasswordHash"),
				Role = (Enums.UserRole)reader.GetInt32("Role"),
				CreatedAt = reader.GetDateTime("CreatedAt")
			};
		}
	}
}