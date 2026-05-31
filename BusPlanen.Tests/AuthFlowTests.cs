using BusRejser.Common.Email;
using BusRejser.Common.Security;
using BusRejser.Features.Auth.DTOs;
using BusRejser.Features.Auth.Services;
using BusRejser.Features.Users.Services;
using BusRejser.Options;
using BusRejserLibrary.Database;
using BusRejserLibrary.Models;
using BusRejserLibrary.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace BusPlanen.Tests;

public class AuthFlowTests
{
	[Fact]
	public async Task Register_Persists_Normalized_Email_And_Hashed_Password()
	{
		using var context = CreateContext();
		var authService = CreateAuthService(context);

		var userId = await authService.Register(" Alice ", " Test ", " Alice@Example.com ", "Secret123!");

		var user = context.Users.Single(x => x.UserId == userId);
		var passwordService = new PasswordService();

		Assert.Equal("Alice", user.FirstName);
		Assert.Equal("Test", user.LastName);
		Assert.Equal("alice@example.com", user.Email);
		Assert.True(passwordService.VerifyPassword("Secret123!", user.PasswordHash));
		Assert.NotEqual("Secret123!", user.PasswordHash);
	}

	[Fact]
	public void Login_Returns_Access_And_Refresh_Tokens_And_Persists_Refresh_Session()
	{
		using var context = CreateContext();
		var passwordService = new PasswordService();

		context.Users.Add(new User
		{
			FirstName = "Alice",
			LastName = "Test",
			Email = "alice@example.com",
			PasswordHash = passwordService.HashPassword("Secret123!"),
			EmailConfirmed = true
		});
		context.SaveChanges();

		var authService = CreateAuthService(context);
		var response = authService.Login("alice@example.com", "Secret123!");

		Assert.False(string.IsNullOrWhiteSpace(response.AccessToken));
		Assert.False(string.IsNullOrWhiteSpace(response.RefreshToken));
		Assert.Equal("Bearer", response.TokenType);
		Assert.Equal("alice@example.com", response.User.Email);
		Assert.Single(context.RefreshTokens);
		Assert.NotNull(context.Users.Single().LastLoginAt);
	}

	[Fact]
	public void Refresh_Rotates_Refresh_Token_And_Revokes_Previous_Session()
	{
		using var context = CreateContext();
		var passwordService = new PasswordService();

		context.Users.Add(new User
		{
			FirstName = "Alice",
			LastName = "Test",
			Email = "alice@example.com",
			PasswordHash = passwordService.HashPassword("Secret123!"),
			EmailConfirmed = true
		});
		context.SaveChanges();

		var authService = CreateAuthService(context);
		var loginResponse = authService.Login("alice@example.com", "Secret123!");

		var refreshResponse = authService.Refresh(loginResponse.RefreshToken);

		Assert.NotEqual(loginResponse.RefreshToken, refreshResponse.RefreshToken);
		Assert.Equal(2, context.RefreshTokens.Count());

		var previousToken = context.RefreshTokens.Single(x => x.TokenHash == TokenHasher.Hash(loginResponse.RefreshToken));
		var newToken = context.RefreshTokens.Single(x => x.TokenHash == TokenHasher.Hash(refreshResponse.RefreshToken));

		Assert.NotNull(previousToken.RevokedAt);
		Assert.Equal(newToken.TokenHash, previousToken.ReplacedByTokenHash);
		Assert.Null(newToken.RevokedAt);
	}

	[Fact]
	public void Logout_Revokes_Refresh_Token()
	{
		using var context = CreateContext();
		var passwordService = new PasswordService();

		context.Users.Add(new User
		{
			FirstName = "Alice",
			LastName = "Test",
			Email = "alice@example.com",
			PasswordHash = passwordService.HashPassword("Secret123!"),
			EmailConfirmed = true
		});
		context.SaveChanges();

		var authService = CreateAuthService(context);
		var loginResponse = authService.Login("alice@example.com", "Secret123!");

		authService.Logout(loginResponse.RefreshToken);

		var storedToken = context.RefreshTokens.Single(x => x.TokenHash == TokenHasher.Hash(loginResponse.RefreshToken));
		Assert.NotNull(storedToken.RevokedAt);
	}

	[Fact]
	public void ChangePassword_Persists_New_Hash_And_Revokes_All_Refresh_Tokens()
	{
		using var context = CreateContext();
		var passwordService = new PasswordService();

		var user = new User
		{
			FirstName = "Alice",
			LastName = "Test",
			Email = "alice@example.com",
			PasswordHash = passwordService.HashPassword("OldSecret123!"),
			EmailConfirmed = true
		};

		context.Users.Add(user);
		context.SaveChanges();

		var authService = CreateAuthService(context);
		authService.Login("alice@example.com", "OldSecret123!");

		var service = new UserService(
			new UserRepository(context),
			passwordService,
			new RefreshTokenRepository(context));

		service.ChangePassword(user.UserId, new ChangePasswordRequest
		{
			CurrentPassword = "OldSecret123!",
			NewPassword = "NewSecret123!"
		});

		var updated = context.Users.Single(x => x.UserId == user.UserId);

		Assert.True(passwordService.VerifyPassword("NewSecret123!", updated.PasswordHash));
		Assert.False(passwordService.VerifyPassword("OldSecret123!", updated.PasswordHash));
		Assert.All(context.RefreshTokens, token => Assert.NotNull(token.RevokedAt));
	}

	[Fact]
	public void ResetPassword_Persists_New_Hash_Marks_Token_Used_And_Revokes_Refresh_Tokens()
	{
		using var context = CreateContext();
		var passwordService = new PasswordService();

		var user = new User
		{
			FirstName = "Alice",
			LastName = "Test",
			Email = "alice@example.com",
			PasswordHash = passwordService.HashPassword("OldSecret123!"),
			EmailConfirmed = true
		};

		context.Users.Add(user);
		context.SaveChanges();

		var authService = CreateAuthService(context);
		authService.Login("alice@example.com", "OldSecret123!");

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

		authService.ResetPassword(rawToken, "NewSecret123!");

		var updatedUser = context.Users.Single(x => x.UserId == user.UserId);
		var updatedToken = context.PasswordResetTokens.Single(x => x.Id == resetToken.Id);

		Assert.True(passwordService.VerifyPassword("NewSecret123!", updatedUser.PasswordHash));
		Assert.NotNull(updatedToken.UsedAt);
		Assert.All(context.RefreshTokens, token => Assert.NotNull(token.RevokedAt));
	}

	private static AuthService CreateAuthService(BusPlanenDbContext context)
	{
		var emailSender = new Mock<IEmailSender>();
		emailSender
			.Setup(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);

		return new AuthService(
			new UserRepository(context),
			new PasswordService(),
			new JwtService(Options.Create(new JwtOptions
			{
				Secret = "test_secret_that_is_long_enough_for_unit_tests",
				Issuer = "BusPlanen.Api.Tests",
				Audience = "BusPlanen.Client.Tests",
				AccessTokenLifetimeMinutes = 60
			})),
			new PasswordResetTokenRepository(context),
			new RefreshTokenRepository(context),
			new EmailVerificationTokenRepository(context),
			new EmailService(emailSender.Object),
			Mock.Of<Microsoft.Extensions.Logging.ILogger<AuthService>>(),
			Options.Create(new FrontendOptions
			{
				BaseUrl = "https://frontend.test",
				PaymentSuccessPath = "/betaling/success",
				PaymentCancelPath = "/betaling/cancel",
				PasswordResetPath = "/reset-password"
			}),
			Options.Create(new AuthOptions
			{
				RefreshTokenLifetimeDays = 14,
				RequireConfirmedEmail = false
			})
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
