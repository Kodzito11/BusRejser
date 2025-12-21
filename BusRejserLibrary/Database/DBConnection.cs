using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;

namespace BusRejserLibrary.Database
{
	public class DBConnection
	{
		private readonly string _connectionString;

		public DBConnection(string connectionString)
		{
			_connectionString = connectionString;
		}

		public MySqlConnection GetConnection()
		{
			return new MySqlConnection(_connectionString);
		}

	}
}
