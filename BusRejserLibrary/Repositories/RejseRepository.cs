using System;
using System.Collections.Generic;
using BusRejserLibrary.Database;
using BusRejserLibrary.Models;
using MySqlConnector;

namespace BusRejserLibrary.Repositories
{
	public class RejseRepository
	{
		private readonly DBConnection _db;

		public RejseRepository(DBConnection db)
		{
			_db = db ?? throw new ArgumentNullException(nameof(db));
		}

		public int Create(Rejse rejse)
		{
			const string sql = @"
				INSERT INTO rejse (title, destination, startAt, endAt, price, maxSeats, busId)
				VALUES (@title, @dest, @startAt, @endAt, @price, @maxSeats, @busId);
				SELECT LAST_INSERT_ID();";

			using var conn = _db.GetConnection();
			conn.Open();

			using var cmd = new MySqlCommand(sql, conn);
			cmd.Parameters.AddWithValue("@title", rejse.Title);
			cmd.Parameters.AddWithValue("@dest", rejse.Destination);
			cmd.Parameters.AddWithValue("@startAt", rejse.StartAt);
			cmd.Parameters.AddWithValue("@endAt", rejse.EndAt);
			cmd.Parameters.AddWithValue("@price", rejse.Price);
			cmd.Parameters.AddWithValue("@maxSeats", rejse.MaxSeats);
			cmd.Parameters.AddWithValue("@busId", (object?)rejse.BusId ?? DBNull.Value);

			var idObj = cmd.ExecuteScalar();
			return Convert.ToInt32(idObj);
		}

		public Rejse? GetById(int id)
		{
			const string sql = @"
				SELECT rejseId, title, destination, startAt, endAt, price, maxSeats, bookedSeats, busId
				FROM rejse
				WHERE rejseId = @id
				LIMIT 1;";

			using var conn = _db.GetConnection();
			conn.Open();

			using var cmd = new MySqlCommand(sql, conn);
			cmd.Parameters.AddWithValue("@id", id);

			using var reader = cmd.ExecuteReader();
			if (!reader.Read()) return null;

			return Map(reader);
		}

		public List<Rejse> GetAll()
		{
			const string sql = @"
				SELECT rejseId, title, destination, startAt, endAt, price, maxSeats, bookedSeats, busId
				FROM rejse
				ORDER BY startAt ASC;";

			var list = new List<Rejse>();

			using var conn = _db.GetConnection();
			conn.Open();

			using var cmd = new MySqlCommand(sql, conn);
			using var reader = cmd.ExecuteReader();

			while (reader.Read())
				list.Add(Map(reader));

			return list;
		}

		public bool Delete(int id)
		{
			const string sql = @"DELETE FROM rejse WHERE rejseId = @id;";

			using var conn = _db.GetConnection();
			conn.Open();

			using var cmd = new MySqlCommand(sql, conn);
			cmd.Parameters.AddWithValue("@id", id);

			return cmd.ExecuteNonQuery() > 0;
		}

		public bool Update(int id, Rejse rejse)
		{
			const string sql = @"
				UPDATE rejse
				SET title = @title,
					destination = @destination,
					startAt = @startAt,
					endAt = @endAt,
					price = @price,
					maxSeats = @maxSeats,
					busId = @busId
				WHERE rejseId = @id;";

			using var conn = _db.GetConnection();
			conn.Open();

			using var cmd = new MySqlCommand(sql, conn);
			cmd.Parameters.AddWithValue("@id", id);
			cmd.Parameters.AddWithValue("@title", rejse.Title);
			cmd.Parameters.AddWithValue("@destination", rejse.Destination);
			cmd.Parameters.AddWithValue("@startAt", rejse.StartAt);
			cmd.Parameters.AddWithValue("@endAt", rejse.EndAt);
			cmd.Parameters.AddWithValue("@price", rejse.Price);
			cmd.Parameters.AddWithValue("@maxSeats", rejse.MaxSeats);
			cmd.Parameters.AddWithValue("@busId", (object?)rejse.BusId ?? DBNull.Value);

			return cmd.ExecuteNonQuery() > 0;
		}

		private static Rejse Map(MySqlDataReader reader)
		{
			var id = reader.GetInt32("rejseId");
			var title = reader.GetString("title");
			var dest = reader.GetString("destination");

			var startAt = DateTime.SpecifyKind(
				reader.GetDateTime("StartAt"),
				DateTimeKind.Utc
			);

			var endAt = DateTime.SpecifyKind(
				reader.GetDateTime("EndAt"),
				DateTimeKind.Utc
			);

			var price = reader.GetDecimal("price");
			var maxSeats = reader.GetInt32("maxSeats");
			var bookedSeats = reader.GetInt32("bookedSeats");
			int? busId = reader.IsDBNull(reader.GetOrdinal("busId")) ? null : reader.GetInt32("busId");

			var r = Rejse.Create(title, dest, startAt, endAt, price, maxSeats, busId);
			r.RejseId = id;
			r.BookedSeats = bookedSeats;
			return r;
		}

		public bool TryReserveSeats(int rejseId, int antalPladser)
		{
			const string sql = @"
			UPDATE rejse
			SET bookedSeats = bookedSeats + @antalPladser
			WHERE rejseId = @rejseId
			  AND bookedSeats + @antalPladser <= maxSeats;";

			using var conn = _db.GetConnection();
			conn.Open();

			using var cmd = new MySqlCommand(sql, conn);
			cmd.Parameters.AddWithValue("@rejseId", rejseId);
			cmd.Parameters.AddWithValue("@antalPladser", antalPladser);

			return cmd.ExecuteNonQuery() > 0;
		}

		public bool ReleaseSeats(int rejseId, int antalPladser)
		{
			const string sql = @"
			UPDATE rejse
			SET bookedSeats = bookedSeats - @antalPladser
			WHERE rejseId = @rejseId
			  AND bookedSeats >= @antalPladser;";

			using var conn = _db.GetConnection();
			conn.Open();

			using var cmd = new MySqlCommand(sql, conn);
			cmd.Parameters.AddWithValue("@rejseId", rejseId);
			cmd.Parameters.AddWithValue("@antalPladser", antalPladser);

			return cmd.ExecuteNonQuery() > 0;
		}
	}
}