using BusRejser.DTOs;
using BusRejser.Options;
using BusRejser.Services;
using BusRejserLibrary.Database;
using BusRejserLibrary.Enums;
using BusRejserLibrary.Models;
using BusRejserLibrary.Repositories;
using BusRejserLibrary.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Stripe.Checkout;
using Xunit;

namespace BusPlanen.Tests;

public class StripeBookingFlowTests
{
	[Fact]
	public void CreateCheckoutSession_UsesConfiguredFrontendBaseUrl_ForStripeRedirects()
	{
		using var context = CreateContext();

		context.Rejser.Add(CreateValidRejseEntity(1));
		context.SaveChanges();

		var bookingRepo = new Mock<IBookingRepository>();
		var rejseRepo = new Mock<IRejseRepository>();
		var userRepo = new Mock<IUserRepository>();
		var bookingLogger = new Mock<ILogger<BookingService>>();
		var stripeLogger = new Mock<ILogger<StripeService>>();
		var stripeClient = new Mock<IStripeCheckoutSessionClient>();

		SessionCreateOptions? capturedOptions = null;

		stripeClient
			.Setup(x => x.Create(It.IsAny<SessionCreateOptions>()))
			.Callback<SessionCreateOptions>(options => capturedOptions = options)
			.Returns(new Session
			{
				Id = "sess_123",
				Url = "https://checkout.stripe.test/session"
			});

		var bookingService = CreateBookingService(bookingRepo, rejseRepo, userRepo, bookingLogger);
		var stripeService = new StripeService(
			new RejseRepository(context),
			bookingService,
			stripeClient.Object,
			Options.Create(new FrontendOptions
			{
				BaseUrl = "https://frontend.example.com",
				PaymentSuccessPath = "/betaling/success",
				PaymentCancelPath = "/betaling/cancel"
			}),
			CreateStripeOptions(),
			stripeLogger.Object
		);

		var url = stripeService.CreateCheckoutSession(new CreateCheckoutSessionRequest
		{
			RejseId = 1,
			AntalPladser = 2,
			KundeNavn = "Test User",
			KundeEmail = "test@test.dk"
		}, userId: null);

		Assert.Equal("https://checkout.stripe.test/session", url);
		Assert.NotNull(capturedOptions);
		Assert.Equal(
			"https://frontend.example.com/betaling/success?session_id={CHECKOUT_SESSION_ID}",
			capturedOptions!.SuccessUrl);
		Assert.Equal(
			"https://frontend.example.com/betaling/cancel",
			capturedOptions.CancelUrl);
	}

	[Fact]
	public void CreateFromStripe_DoesNothing_WhenDuplicateBookingAppearsDuringConcurrentCreate()
	{
		var bookingRepo = new Mock<IBookingRepository>();
		var rejseRepo = new Mock<IRejseRepository>();
		var userRepo = new Mock<IUserRepository>();
		var logger = new Mock<ILogger<BookingService>>();

		var request = new StripeWebhookBookingRequest
		{
			RejseId = 1,
			AntalPladser = 2,
			KundeNavn = "Test User",
			KundeEmail = "test@test.dk",
			StripeSessionId = "sess_123",
			StripePaymentIntentId = "pi_123",
			TotalPrice = 200m
		};

		var existingBooking = Booking.Restore(
			bookingId: 42,
			rejseId: 1,
			userId: null,
			bookingReference: "BP-EXIST42",
			kundeNavn: "Test User",
			kundeEmail: "test@test.dk",
			antalPladser: 2,
			totalPrice: 200m,
			status: BookingStatus.Paid,
			createdAt: DateTime.UtcNow.AddMinutes(-5),
			paidAt: DateTime.UtcNow.AddMinutes(-5),
			stripeSessionId: "sess_123",
			stripePaymentIntentId: "pi_123"
		);

		bookingRepo
			.SetupSequence(x => x.GetByStripeSessionId("sess_123"))
			.Returns((Booking?)null)
			.Returns(existingBooking);

		bookingRepo
			.Setup(x => x.Create(It.IsAny<Booking>()))
			.Throws(new DbUpdateException("Duplicate Stripe session", new Exception("duplicate")));

		rejseRepo.Setup(x => x.GetById(1)).Returns(CreateValidRejse(1));
		rejseRepo.Setup(x => x.TryReserveSeats(1, 2)).Returns(true);
		rejseRepo.Setup(x => x.ReleaseSeats(1, 2)).Returns(true);

		var service = CreateBookingService(bookingRepo, rejseRepo, userRepo, logger);

		service.CreateFromStripe(request);

		bookingRepo.Verify(x => x.Create(It.IsAny<Booking>()), Times.Once);
		rejseRepo.Verify(x => x.ReleaseSeats(1, 2), Times.Once);
	}

	[Fact]
	public void GetCheckoutStatus_ReturnsProcessing_WhenPaidSessionHasNoBookingYet()
	{
		using var context = CreateContext();

		var bookingRepo = new Mock<IBookingRepository>();
		var rejseRepo = new Mock<IRejseRepository>();
		var userRepo = new Mock<IUserRepository>();
		var bookingLogger = new Mock<ILogger<BookingService>>();
		var stripeLogger = new Mock<ILogger<StripeService>>();
		var stripeClient = new Mock<IStripeCheckoutSessionClient>();

		bookingRepo
			.Setup(x => x.GetByStripeSessionId("sess_123"))
			.Returns((Booking?)null);

		stripeClient
			.Setup(x => x.Get("sess_123"))
			.Returns(new Session
			{
				Id = "sess_123",
				PaymentStatus = "paid"
			});

		var bookingService = CreateBookingService(bookingRepo, rejseRepo, userRepo, bookingLogger);
		var stripeService = new StripeService(
			new RejseRepository(context),
			bookingService,
			stripeClient.Object,
			Options.Create(new FrontendOptions
			{
				BaseUrl = "https://frontend.test",
				PaymentSuccessPath = "/betaling/success",
				PaymentCancelPath = "/betaling/cancel"
			}),
			CreateStripeOptions(),
			stripeLogger.Object
		);

		var result = stripeService.GetCheckoutStatus("sess_123");

		Assert.True(result.IsPaid);
		Assert.False(result.BookingExists);
		Assert.Equal("processing", result.Status);
		bookingRepo.Verify(x => x.Create(It.IsAny<Booking>()), Times.Never);
	}

	private static BookingService CreateBookingService(
		Mock<IBookingRepository> bookingRepo,
		Mock<IRejseRepository> rejseRepo,
		Mock<IUserRepository> userRepo,
		Mock<ILogger<BookingService>> logger)
	{
		return new BookingService(
			bookingRepo.Object,
			rejseRepo.Object,
			userRepo.Object,
			logger.Object
		);
	}

	private static Rejse CreateValidRejse(int rejseId, int maxSeats = 50, int bookedSeats = 10)
	{
		var rejse = Rejse.Create(
			"Test",
			"København",
			"Danmark",
			"København",
			DateTime.UtcNow.AddDays(2),
			DateTime.UtcNow.AddDays(3),
			100m,
			maxSeats,
			null,
			null,
			null,
			null,
			false,
			true
		);

		rejse.RejseId = rejseId;
		rejse.BookedSeats = bookedSeats;
		return rejse;
	}

	private static Rejse CreateValidRejseEntity(int rejseId, int maxSeats = 50)
	{
		var rejse = Rejse.Create(
			"Test",
			"Copenhagen",
			"Denmark",
			"Copenhagen",
			DateTime.UtcNow.AddDays(2),
			DateTime.UtcNow.AddDays(3),
			100m,
			maxSeats,
			null,
			null,
			null,
			null,
			false,
			true
		);

		rejse.RejseId = rejseId;
		return rejse;
	}

	private static IOptions<StripeOptions> CreateStripeOptions()
	{
		return Options.Create(new StripeOptions
		{
			SecretKey = "sk_test_1234567890",
			WebhookSecret = "whsec_test_1234567890"
		});
	}

	private static BusPlanenDbContext CreateContext()
	{
		var options = new DbContextOptionsBuilder<BusPlanenDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;

		return new BusPlanenDbContext(options);
	}
}
