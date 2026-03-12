using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

namespace BusRejser.Services
{
	public class PasswordService
	{
		public string HashPassword(string password)
		{
			byte[] salt = RandomNumberGenerator.GetBytes(16);

			using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
			{
				Salt = salt,
				DegreeOfParallelism = 4,
				Iterations = 3,
				MemorySize = 65536
			};

			byte[] hash = argon2.GetBytes(32);

			return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
		}

		public bool VerifyPassword(string password, string storedHash)
		{
			var parts = storedHash.Split('.');
			if (parts.Length != 2) return false;

			byte[] salt = Convert.FromBase64String(parts[0]);
			byte[] expectedHash = Convert.FromBase64String(parts[1]);

			using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
			{
				Salt = salt,
				DegreeOfParallelism = 4,
				Iterations = 3,
				MemorySize = 65536
			};

			byte[] actualHash = argon2.GetBytes(32);

			return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
		}
	}
}