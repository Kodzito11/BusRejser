using BusRejser.DTOs;
using BusRejser.Security;
using BusRejser.Services;
using BusRejserLibrary.Database;
using BusRejserLibrary.Models;
using BusRejserLibrary.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace BusPlanen.Tests;

public class AuthFlowTests
{
	[Fact]
	public void Register_Persists_Normalized_Email_Username_And_Hashed_Password()
	{
		using var context = CreateContext();
		var authService = CreateAuthService(context);

		var userId = authService.Register(" alice ", " Alice@Example.com ", "Secret123!");

		var user = context.Users.Single(x => x.UserId == userId);
		var passwordService = new PasswordService();

		Assert.Equal("alice", user.Username);
		Assert.Equal("alice@example.com", user.Email);
		Assert.True(passwordService.VerifyPassword("Secret123!", user.PasswordHash));
		Assert.NotEqual("Secret123!", user.PasswordHash);
	}

	[Fact]
	public void ChangePassword_Persists_New_Hash()
	{
		using var context = CreateContext();
		var passwordService = new PasswordService();

		var user = new User
		{
			Username = "alice",
			Email = "alice@example.com",
			PasswordHash = passwordService.HashPassword("OldSecret123!")
		};

		context.Users.Add(user);
		context.SaveChanges();

		var service = new UserService(new UserRepository(context), passwordService);

		service.ChangePassword(user.UserId, new ChangePasswordRequest
		{
			CurrentPassword = "OldSecret123!",
			NewPassword = "NewSecret123!"
		});

		var updated = context.Users.Single(x => x.UserId == user.UserId);

		Assert.True(passwordService.VerifyPassword("NewSecret123!", updated.PasswordHash));
		Assert.False(passwordService.VerifyPassword("OldSecret123!", updated.PasswordHash));
	}

	[Fact]
	public void ResetPassword_Persists_New_Hash_And_Marks_Token_Used()
	{
		using var context = CreateContext();
		var passwordService = new PasswordService();

		var user = new User
		{
			Username = "alice",
			Email = "alice@example.com",
			PasswordHash = passwordService.HashPassword("OldSecret123!")
		};

		context.Users.Add(user);
		context.SaveChanges();

		const string rawToken = "reset-token-123";

		var resetToken = new PasswordResetToken
		{
			UserId = user.UserId,
			TokenHash = TokenHasher.Hash(rawToken),
			CreatedAt = DateTime.UtcNow,
			ExpiresAt = DateTime.UtcNow.AddMinutes(30)
		};

		context.PasswordResetTokens.Add(resetToken);
		context.SaveChanges();

		var authService = CreateAuthService(context);
		authService.ResetPassword(rawToken, "NewSecret123!");

		var updatedUser = context.Users.Single(x => x.UserId == user.UserId);
		var updatedToken = context.PasswordResetTokens.Single(x => x.Id == resetToken.Id);

		Assert.True(passwordService.VerifyPassword("NewSecret123!", updatedUser.PasswordHash));
		Assert.NotNull(updatedToken.UsedAt);
	}

	private static AuthService CreateAuthService(BusPlanenDbContext context)
	{
		var config = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>
			{
				["Email:From"] = "noreply@test.local",
				["Email:Host"] = "localhost",
				["Email:Port"] = "25",
				["Email:Username"] = "test",
				["Email:Password"] = "test"
			})
			.Build();

		return new AuthService(
			new UserRepository(context),
			new PasswordService(),
			new JwtService("test_secret_that_is_long_enough_for_unit_tests"),
			new PasswordResetTokenRepository(context),
			new EmailService(config)
		);
	}

	private static BusPlanenDbContext CreateContext()
	{
		var options = new DbContextOptionsBuilder<BusPlanenDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;

		return new BusPlanenDbContext(options);
	}
}
