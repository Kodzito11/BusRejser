using BusRejserLibrary.Database;
using BusRejserLibrary.Models;
using MySqlConnector;

namespace BusRejserLibrary.Repositories
{
	public class BookingRepository
	{
		private readonly DBConnection _db;

		public BookingRepository(DBConnection db)
		{
			_db = db ?? throw new ArgumentNullException(nameof(db));
		}

		public int Create(Booking booking)
		{
			const string sql = @"
				INSERT INTO booking 
				(rejseId, userId, bookingReference, kundeNavn, kundeEmail, antalPladser, status, createdAt)
				VALUES 
				(@rejseId, @userId, @bookingReference, @kundeNavn, @kundeEmail, @antalPladser, @status, @createdAt);
				SELECT LAST_INSERT_ID();";

			using var conn = _db.GetConnection();
			conn.Open();

			using var cmd = new MySqlCommand(sql, conn);
			cmd.Parameters.AddWithValue("@rejseId", booking.RejseId);
			cmd.Parameters.AddWithValue("@userId", (object?)booking.UserId ?? DBNull.Value);
			cmd.Parameters.AddWithValue("@bookingReference", booking.BookingReference);
			cmd.Parameters.AddWithValue("@kundeNavn", booking.KundeNavn);
			cmd.Parameters.AddWithValue("@kundeEmail", booking.KundeEmail);
			cmd.Parameters.AddWithValue("@antalPladser", booking.AntalPladser);
			cmd.Parameters.AddWithValue("@status", (int)booking.Status);
			cmd.Parameters.AddWithValue("@createdAt", booking.CreatedAt);

			var idObj = cmd.ExecuteScalar();
			var newId = Convert.ToInt32(idObj);

			booking.BookingId = newId;
			return newId;
		}

		public Booking? GetById(int id)
		{
			const string sql = @"
				SELECT bookingId, rejseId, userId, bookingReference, kundeNavn, kundeEmail, antalPladser, status, createdAt
				FROM booking
				WHERE bookingId = @id
				LIMIT 1;";

			using var conn = _db.GetConnection();
			conn.Open();

			using var cmd = new MySqlCommand(sql, conn);
			cmd.Parameters.AddWithValue("@id", id);

			using var reader = cmd.ExecuteReader();
			if (!reader.Read()) return null;

			return Map(reader);
		}

		public List<Booking> GetByRejseId(int rejseId)
		{
			const string sql = @"
				SELECT bookingId, rejseId, userId, bookingReference, kundeNavn, kundeEmail, antalPladser, status, createdAt
				FROM booking
				WHERE rejseId = @rejseId
				ORDER BY createdAt DESC;";

			var list = new List<Booking>();

			using var conn = _db.GetConnection();
			conn.Open();

			using var cmd = new MySqlCommand(sql, conn);
			cmd.Parameters.AddWithValue("@rejseId", rejseId);

			using var reader = cmd.ExecuteReader();
			while (reader.Read())
				list.Add(Map(reader));

			return list;
		}

		public int GetTotalBookedSeatsForRejse(int rejseId)
		{
			const string sql = @"
				SELECT COALESCE(SUM(antalPladser), 0)
				FROM booking
				WHERE rejseId = @rejseId
				  AND status = 0;";

			using var conn = _db.GetConnection();
			conn.Open();

			using var cmd = new MySqlCommand(sql, conn);
			cmd.Parameters.AddWithValue("@rejseId", rejseId);

			var result = cmd.ExecuteScalar();
			return Convert.ToInt32(result);
		}

		public List<Booking> GetByUserId(int userId)
		{
			const string sql = @"
				SELECT bookingId, rejseId, userId, bookingReference, kundeNavn, kundeEmail, antalPladser, status, createdAt
				FROM booking
				WHERE userId = @userId
				ORDER BY createdAt DESC;";

			var list = new List<Booking>();

			using var conn = _db.GetConnection();
			conn.Open();

			using var cmd = new MySqlCommand(sql, conn);
			cmd.Parameters.AddWithValue("@userId", userId);

			using var reader = cmd.ExecuteReader();
			while (reader.Read())
				list.Add(Map(reader));

			return list;
		}

		public bool Cancel(int id)
		{
			const string sql = @"
				UPDATE booking
				SET status = 1
				WHERE bookingId = @id AND status = 0;";

			using var conn = _db.GetConnection();
			conn.Open();

			using var cmd = new MySqlCommand(sql, conn);
			cmd.Parameters.AddWithValue("@id", id);

			return cmd.ExecuteNonQuery() > 0;
		}

		public bool Reactivate(int id)
		{
			const string sql = @"
				UPDATE booking
				SET status = 0
				WHERE bookingId = @id AND status = 1;";

			using var conn = _db.GetConnection();
			conn.Open();

			using var cmd = new MySqlCommand(sql, conn);
			cmd.Parameters.AddWithValue("@id", id);

			return cmd.ExecuteNonQuery() > 0;
		}

		private static Booking Map(MySqlDataReader reader)
		{
			int? userId = reader.IsDBNull(reader.GetOrdinal("userId"))
				? null
				: reader.GetInt32("userId");

			return Booking.Restore(
				reader.GetInt32("bookingId"),
				reader.GetInt32("rejseId"),
				userId,
				reader.GetString("bookingReference"),
				reader.GetString("kundeNavn"),
				reader.GetString("kundeEmail"),
				reader.GetInt32("antalPladser"),
				(BusRejserLibrary.Enums.BookingStatus)reader.GetInt32("status"),
				DateTime.SpecifyKind(reader.GetDateTime("createdAt"), DateTimeKind.Utc)
			);
		}
	}
}