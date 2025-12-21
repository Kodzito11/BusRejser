using System;
using System.Collections.Generic;
using BusRejserLibrary.Database;
using MySqlConnector;

namespace BusRejserLibrary.Repositories
{
	public class BusFacilitetRepository
	{
		private readonly DBConnection _db;

		public BusFacilitetRepository(DBConnection db)
		{
			_db = db ?? throw new ArgumentNullException(nameof(db));
		}

		public bool Add(int busId, int facilitetId)
		{
			const string sql = @"
			INSERT IGNORE INTO bus_facilitet (busId, facilitetId)
			VALUES (@busId, @facilitetId);";

			using var conn = _db.GetConnection();
			conn.Open();

			using var cmd = new MySqlCommand(sql, conn);
			cmd.Parameters.AddWithValue("@busId", busId);
			cmd.Parameters.AddWithValue("@facilitetId", facilitetId);

			return cmd.ExecuteNonQuery() > 0;
		}

		public bool Remove(int busId, int facilitetId)
		{
			const string sql = @"
			DELETE FROM bus_facilitet
			WHERE busId = @busId AND facilitetId = @facilitetId;";

			using var conn = _db.GetConnection();
			conn.Open();

			using var cmd = new MySqlCommand(sql, conn);
			cmd.Parameters.AddWithValue("@busId", busId);
			cmd.Parameters.AddWithValue("@facilitetId", facilitetId);

			return cmd.ExecuteNonQuery() > 0;
		}

		public List<int> GetFacilitetIdsForBus(int busId)
		{
			const string sql = @"
			SELECT facilitetId
			FROM bus_facilitet
			WHERE busId = @busId;";

			var list = new List<int>();

			using var conn = _db.GetConnection();
			conn.Open();

			using var cmd = new MySqlCommand(sql, conn);
			cmd.Parameters.AddWithValue("@busId", busId);

			using var reader = cmd.ExecuteReader();
			while (reader.Read())
				list.Add(reader.GetInt32("facilitetId"));

			return list;
		}
	}
}
