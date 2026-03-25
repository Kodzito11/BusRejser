using BusRejserLibrary.Database;
using BusRejserLibrary.Enums;
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
				(rejseId, userId, bookingReference, kundeNavn, kundeEmail, antalPladser, totalPrice, status, createdAt, paidAt, stripeSessionId, stripePaymentIntentId)
				VALUES
				(@rejseId, @userId, @bookingReference, @kundeNavn, @kundeEmail, @antalPladser, @totalPrice, @status, @createdAt, @paidAt, @stripeSessionId, @stripePaymentIntentId);
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
			cmd.Parameters.AddWithValue("@totalPrice", booking.TotalPrice);
			cmd.Parameters.AddWithValue("@status", (int)booking.Status);
			cmd.Parameters.AddWithValue("@createdAt", booking.CreatedAt);
			cmd.Parameters.AddWithValue("@paidAt", (object?)booking.PaidAt ?? DBNull.Value);
			cmd.Parameters.AddWithValue("@stripeSessionId", (object?)booking.StripeSessionId ?? DBNull.Value);
			cmd.Parameters.AddWithValue("@stripePaymentIntentId", (object?)booking.StripePaymentIntentId ?? DBNull.Value);

			var idObj = cmd.ExecuteScalar();
			var newId = Convert.ToInt32(idObj);

			booking.BookingId = newId;
			return newId;
		}

		public List<Booking> GetAll()
		{
			var bookings = new List<Booking>();

			using var conn = _db.GetConnection();
			conn.Open();

			using var cmd = conn.CreateCommand();
			cmd.CommandText = @"
				SELECT 
					bookingId,
					rejseId,
					userId,
					bookingReference,
					kundeNavn,
					kundeEmail,
					antalPladser,
					totalPrice,
					status,
					createdAt,
					paidAt,
					stripeSessionId,
					stripePaymentIntentId
				FROM booking
				ORDER BY bookingId DESC";

			using var reader = cmd.ExecuteReader();

			while (reader.Read())
			{
				bookings.Add(Map(reader));
			}

			return bookings;
		}

		public Booking? GetById(int id)
		{
			const string sql = @"
				SELECT 
					bookingId,
					rejseId,
					userId,
					bookingReference,
					kundeNavn,
					kundeEmail,
					antalPladser,
					totalPrice,
					status,
					createdAt,
					paidAt,
					stripeSessionId,
					stripePaymentIntentId
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

		public Booking? GetByStripeSessionId(string stripeSessionId)
		{
			const string sql = @"
				SELECT
					bookingId,
					rejseId,
					userId,
					bookingReference,
					kundeNavn,
					kundeEmail,
					antalPladser,
					totalPrice,
					status,
					createdAt,
					paidAt,
					stripeSessionId,
					stripePaymentIntentId
				FROM booking
				WHERE stripeSessionId = @stripeSessionId
				LIMIT 1;";

			using var conn = _db.GetConnection();
			conn.Open();

			using var cmd = new MySqlCommand(sql, conn);
			cmd.Parameters.AddWithValue("@stripeSessionId", stripeSessionId);

			using var reader = cmd.ExecuteReader();
			if (!reader.Read()) return null;

			return Map(reader);
		}

		public List<Booking> GetByRejseId(int rejseId)
		{
			const string sql = @"
				SELECT 
					bookingId,
					rejseId,
					userId,
					bookingReference,
					kundeNavn,
					kundeEmail,
					antalPladser,
					totalPrice,
					status,
					createdAt,
					paidAt,
					stripeSessionId,
					stripePaymentIntentId
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
				  AND status = @status;";

			using var conn = _db.GetConnection();
			conn.Open();

			using var cmd = new MySqlCommand(sql, conn);
			cmd.Parameters.AddWithValue("@rejseId", rejseId);
			cmd.Parameters.AddWithValue("@status", (int)BookingStatus.Paid);

			var result = cmd.ExecuteScalar();
			return Convert.ToInt32(result);
		}

		public List<Booking> GetByUserId(int userId)
		{
			const string sql = @"
				SELECT 
					bookingId,
					rejseId,
					userId,
					bookingReference,
					kundeNavn,
					kundeEmail,
					antalPladser,
					totalPrice,
					status,
					createdAt,
					paidAt,
					stripeSessionId,
					stripePaymentIntentId
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

		public bool CancelAndReleaseSeats(int bookingId)
		{
			using var conn = _db.GetConnection();
			conn.Open();

			using var transaction = conn.BeginTransaction();

			try
			{
				const string selectSql = @"
					SELECT rejseId, antalPladser, status
					FROM booking
					WHERE bookingId = @bookingId
					FOR UPDATE;";

				int rejseId;
				int antalPladser;
				int status;

				using (var selectCmd = new MySqlCommand(selectSql, conn, transaction))
				{
					selectCmd.Parameters.AddWithValue("@bookingId", bookingId);

					using var reader = selectCmd.ExecuteReader();

					if (!reader.Read())
					{
						transaction.Rollback();
						return false;
					}

					rejseId = reader.GetInt32("rejseId");
					antalPladser = reader.GetInt32("antalPladser");
					status = reader.GetInt32("status");
				}

				if ((BookingStatus)status == BookingStatus.Cancelled)
				{
					transaction.Commit();
					return true;
				}

				const string cancelSql = @"
					UPDATE booking
					SET status = @cancelledStatus
					WHERE bookingId = @bookingId
					  AND status = @paidStatus;";

				using (var cancelCmd = new MySqlCommand(cancelSql, conn, transaction))
				{
					cancelCmd.Parameters.AddWithValue("@bookingId", bookingId);
					cancelCmd.Parameters.AddWithValue("@cancelledStatus", (int)BookingStatus.Cancelled);
					cancelCmd.Parameters.AddWithValue("@paidStatus", (int)BookingStatus.Paid);

					var cancelRows = cancelCmd.ExecuteNonQuery();
					if (cancelRows == 0)
					{
						transaction.Rollback();
						return false;
					}
				}

				const string releaseSeatsSql = @"
					UPDATE rejse
					SET bookedSeats = bookedSeats - @antalPladser
					WHERE rejseId = @rejseId
					  AND bookedSeats >= @antalPladser;";

				using (var seatsCmd = new MySqlCommand(releaseSeatsSql, conn, transaction))
				{
					seatsCmd.Parameters.AddWithValue("@rejseId", rejseId);
					seatsCmd.Parameters.AddWithValue("@antalPladser", antalPladser);

					var seatRows = seatsCmd.ExecuteNonQuery();
					if (seatRows == 0)
					{
						transaction.Rollback();
						return false;
					}
				}

				transaction.Commit();
				return true;
			}
			catch
			{
				transaction.Rollback();
				throw;
			}
		}

		public bool ReactivateAndReserveSeats(int bookingId)
		{
			using var conn = _db.GetConnection();
			conn.Open();

			using var transaction = conn.BeginTransaction();

			try
			{
				const string selectSql = @"
					SELECT rejseId, antalPladser, status
					FROM booking
					WHERE bookingId = @bookingId
					FOR UPDATE;";

				int rejseId;
				int antalPladser;
				int status;

				using (var selectCmd = new MySqlCommand(selectSql, conn, transaction))
				{
					selectCmd.Parameters.AddWithValue("@bookingId", bookingId);

					using var reader = selectCmd.ExecuteReader();

					if (!reader.Read())
					{
						transaction.Rollback();
						return false;
					}

					rejseId = reader.GetInt32("rejseId");
					antalPladser = reader.GetInt32("antalPladser");
					status = reader.GetInt32("status");
				}

				if ((BookingStatus)status == BookingStatus.Paid)
				{
					transaction.Commit();
					return true;
				}

				const string reserveSeatsSql = @"
					UPDATE rejse
					SET bookedSeats = bookedSeats + @antalPladser
					WHERE rejseId = @rejseId
					  AND bookedSeats + @antalPladser <= maxSeats;";

				using (var seatsCmd = new MySqlCommand(reserveSeatsSql, conn, transaction))
				{
					seatsCmd.Parameters.AddWithValue("@rejseId", rejseId);
					seatsCmd.Parameters.AddWithValue("@antalPladser", antalPladser);

					var seatRows = seatsCmd.ExecuteNonQuery();
					if (seatRows == 0)
					{
						transaction.Rollback();
						return false;
					}
				}

				const string reactivateSql = @"
					UPDATE booking
					SET status = @paidStatus
					WHERE bookingId = @bookingId
					  AND status = @cancelledStatus;";

				using (var reactivateCmd = new MySqlCommand(reactivateSql, conn, transaction))
				{
					reactivateCmd.Parameters.AddWithValue("@bookingId", bookingId);
					reactivateCmd.Parameters.AddWithValue("@paidStatus", (int)BookingStatus.Paid);
					reactivateCmd.Parameters.AddWithValue("@cancelledStatus", (int)BookingStatus.Cancelled);

					var reactivateRows = reactivateCmd.ExecuteNonQuery();
					if (reactivateRows == 0)
					{
						transaction.Rollback();
						return false;
					}
				}

				transaction.Commit();
				return true;
			}
			catch
			{
				transaction.Rollback();
				throw;
			}
		}

		private static Booking Map(MySqlDataReader reader)
		{
			int? userId = reader.IsDBNull(reader.GetOrdinal("userId"))
				? null
				: reader.GetInt32("userId");

			DateTime? paidAt = reader.IsDBNull(reader.GetOrdinal("paidAt"))
				? null
				: DateTime.SpecifyKind(reader.GetDateTime("paidAt"), DateTimeKind.Utc);

			string? stripeSessionId = reader.IsDBNull(reader.GetOrdinal("stripeSessionId"))
				? null
				: reader.GetString("stripeSessionId");

			string? stripePaymentIntentId = reader.IsDBNull(reader.GetOrdinal("stripePaymentIntentId"))
				? null
				: reader.GetString("stripePaymentIntentId");

			return Booking.Restore(
				reader.GetInt32("bookingId"),
				reader.GetInt32("rejseId"),
				userId,
				reader.GetString("bookingReference"),
				reader.GetString("kundeNavn"),
				reader.GetString("kundeEmail"),
				reader.GetInt32("antalPladser"),
				reader.GetDecimal("totalPrice"),
				(BookingStatus)reader.GetInt32("status"),
				DateTime.SpecifyKind(reader.GetDateTime("createdAt"), DateTimeKind.Utc),
				paidAt,
				stripeSessionId,
				stripePaymentIntentId
			);
		}
	}
}