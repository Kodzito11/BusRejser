using BusRejser.Exceptions;
using BusRejserLibrary.Enums;
using BusRejserLibrary.Models;
using BusRejserLibrary.Repositories;
using BusRejserLibrary.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BusPlanen.Tests;

public class BookingServiceTests
{
	private static BookingService CreateService(
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

	private static Rejse CreateValidRejse(int rejseId = 1, int maxSeats = 50, int bookedSeats = 10)
	{
		var rejse = Rejse.Create(
			"Test",
			"København",
			DateTime.UtcNow.AddDays(2),
			DateTime.UtcNow.AddDays(3),
			100,
			maxSeats,
			null
		);

		rejse.RejseId = rejseId;
		rejse.BookedSeats = bookedSeats;
		return rejse;
	}

	private static Booking CreatePaidBooking(
		int rejseId = 1,
		int? userId = null,
		int antalPladser = 2,
		decimal totalPrice = 200m)
	{
		var booking = Booking.Create(
			rejseId,
			userId,
			"Test User",
			"test@test.dk",
			antalPladser,
			totalPrice
		);

		booking.MarkAsPaid("sess_123", "pi_123");
		return booking;
	}

	[Fact]
	public void Create_WhenSeatsAvailable_CreatesBooking()
	{
		// Arrange
		var bookingRepo = new Mock<IBookingRepository>();
		var rejseRepo = new Mock<IRejseRepository>();
		var userRepo = new Mock<IUserRepository>();
		var logger = new Mock<ILogger<BookingService>>();

		var rejse = CreateValidRejse(rejseId: 1);
		rejseRepo.Setup(x => x.GetById(1)).Returns(rejse);
		rejseRepo.Setup(x => x.TryReserveSeats(1, 2)).Returns(true);
		bookingRepo.Setup(x => x.Create(It.IsAny<Booking>())).Returns(1);

		var service = CreateService(bookingRepo, rejseRepo, userRepo, logger);
		var booking = CreatePaidBooking(rejseId: 1, antalPladser: 2, totalPrice: 200m);

		// Act
		var id = service.Create(booking);

		// Assert
		Assert.Equal(1, id);
		rejseRepo.Verify(x => x.GetById(1), Times.Once);
		rejseRepo.Verify(x => x.TryReserveSeats(1, 2), Times.Once);
		bookingRepo.Verify(x => x.Create(It.IsAny<Booking>()), Times.Once);
		rejseRepo.Verify(x => x.ReleaseSeats(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
	}

	[Fact]
	public void Create_WhenRejseDoesNotExist_ThrowsNotFoundException()
	{
		// Arrange
		var bookingRepo = new Mock<IBookingRepository>();
		var rejseRepo = new Mock<IRejseRepository>();
		var userRepo = new Mock<IUserRepository>();
		var logger = new Mock<ILogger<BookingService>>();

		rejseRepo.Setup(x => x.GetById(999)).Returns((Rejse?)null);

		var service = CreateService(bookingRepo, rejseRepo, userRepo, logger);
		var booking = CreatePaidBooking(rejseId: 999, antalPladser: 2, totalPrice: 200m);

		// Act + Assert
		Assert.Throws<NotFoundException>(() => service.Create(booking));

		rejseRepo.Verify(x => x.GetById(999), Times.Once);
		rejseRepo.Verify(x => x.TryReserveSeats(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
		bookingRepo.Verify(x => x.Create(It.IsAny<Booking>()), Times.Never);
	}

	[Fact]
	public void Create_WhenBookingIsNotPaid_ThrowsValidationException()
	{
		// Arrange
		var bookingRepo = new Mock<IBookingRepository>();
		var rejseRepo = new Mock<IRejseRepository>();
		var userRepo = new Mock<IUserRepository>();
		var logger = new Mock<ILogger<BookingService>>();

		var rejse = CreateValidRejse(rejseId: 1);
		rejseRepo.Setup(x => x.GetById(1)).Returns(rejse);

		var service = CreateService(bookingRepo, rejseRepo, userRepo, logger);

		var booking = Booking.Create(
			1,
			null,
			"Test User",
			"test@test.dk",
			2,
			200m
		);

		// Act + Assert
		Assert.Throws<ValidationException>(() => service.Create(booking));

		rejseRepo.Verify(x => x.GetById(1), Times.Once);
		rejseRepo.Verify(x => x.TryReserveSeats(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
		bookingRepo.Verify(x => x.Create(It.IsAny<Booking>()), Times.Never);
	}

	[Fact]
	public void Create_WhenNoSeatsAvailable_ThrowsValidationException()
	{
		// Arrange
		var bookingRepo = new Mock<IBookingRepository>();
		var rejseRepo = new Mock<IRejseRepository>();
		var userRepo = new Mock<IUserRepository>();
		var logger = new Mock<ILogger<BookingService>>();

		var rejse = CreateValidRejse(rejseId: 1, maxSeats: 10, bookedSeats: 9);
		rejseRepo.Setup(x => x.GetById(1)).Returns(rejse);
		rejseRepo.Setup(x => x.TryReserveSeats(1, 5)).Returns(false);

		var service = CreateService(bookingRepo, rejseRepo, userRepo, logger);
		var booking = CreatePaidBooking(rejseId: 1, antalPladser: 5, totalPrice: 500m);

		// Act + Assert
		Assert.Throws<ValidationException>(() => service.Create(booking));

		rejseRepo.Verify(x => x.GetById(1), Times.Once);
		rejseRepo.Verify(x => x.TryReserveSeats(1, 5), Times.Once);
		bookingRepo.Verify(x => x.Create(It.IsAny<Booking>()), Times.Never);
		rejseRepo.Verify(x => x.ReleaseSeats(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
	}

	[Fact]
	public void Create_WhenRepositoryFailsAfterSeatReservation_ReleasesSeatsAndRethrows()
	{
		// Arrange
		var bookingRepo = new Mock<IBookingRepository>();
		var rejseRepo = new Mock<IRejseRepository>();
		var userRepo = new Mock<IUserRepository>();
		var logger = new Mock<ILogger<BookingService>>();

		var rejse = CreateValidRejse(rejseId: 1);
		rejseRepo.Setup(x => x.GetById(1)).Returns(rejse);
		rejseRepo.Setup(x => x.TryReserveSeats(1, 2)).Returns(true);

		bookingRepo
			.Setup(x => x.Create(It.IsAny<Booking>()))
			.Throws(new Exception("DB failed"));

		var service = CreateService(bookingRepo, rejseRepo, userRepo, logger);
		var booking = CreatePaidBooking(rejseId: 1, antalPladser: 2, totalPrice: 200m);

		// Act + Assert
		var ex = Assert.Throws<Exception>(() => service.Create(booking));

		Assert.Equal("DB failed", ex.Message);
		rejseRepo.Verify(x => x.TryReserveSeats(1, 2), Times.Once);
		rejseRepo.Verify(x => x.ReleaseSeats(1, 2), Times.Once);
		bookingRepo.Verify(x => x.Create(It.IsAny<Booking>()), Times.Once);
	}

	[Fact]
	public void Cancel_WhenStaffAndBookingIsPaid_CallsRepositoryTransactionMethod()
	{
		// Arrange
		var bookingRepo = new Mock<IBookingRepository>();
		var rejseRepo = new Mock<IRejseRepository>();
		var userRepo = new Mock<IUserRepository>();
		var logger = new Mock<ILogger<BookingService>>();

		var booking = Booking.Restore(
			bookingId: 1,
			rejseId: 10,
			userId: 123,
			bookingReference: "BP-TEST123",
			kundeNavn: "Test User",
			kundeEmail: "test@test.dk",
			antalPladser: 2,
			totalPrice: 500m,
			status: BookingStatus.Paid,
			createdAt: DateTime.UtcNow.AddDays(-1),
			paidAt: DateTime.UtcNow.AddDays(-1),
			stripeSessionId: "sess_123",
			stripePaymentIntentId: "pi_123"
		);

		bookingRepo.Setup(x => x.GetById(1)).Returns(booking);
		bookingRepo.Setup(x => x.CancelAndReleaseSeats(1)).Returns(true);

		var service = CreateService(bookingRepo, rejseRepo, userRepo, logger);

		// Act
		var result = service.Cancel(1, actingUserId: null, isStaff: true);

		// Assert
		Assert.True(result);
		bookingRepo.Verify(x => x.GetById(1), Times.Once);
		bookingRepo.Verify(x => x.CancelAndReleaseSeats(1), Times.Once);
	}

	[Fact]
	public void Cancel_WhenBookingDoesNotExist_ThrowsNotFoundException()
	{
		// Arrange
		var bookingRepo = new Mock<IBookingRepository>();
		var rejseRepo = new Mock<IRejseRepository>();
		var userRepo = new Mock<IUserRepository>();
		var logger = new Mock<ILogger<BookingService>>();

		bookingRepo.Setup(x => x.GetById(999)).Returns((Booking?)null);

		var service = CreateService(bookingRepo, rejseRepo, userRepo, logger);

		// Act + Assert
		Assert.Throws<NotFoundException>(() => service.Cancel(999, actingUserId: 123, isStaff: false));

		bookingRepo.Verify(x => x.GetById(999), Times.Once);
		bookingRepo.Verify(x => x.CancelAndReleaseSeats(It.IsAny<int>()), Times.Never);
	}

	[Fact]
	public void Cancel_WhenActingUserOwnsBooking_CallsRepositoryTransactionMethod()
	{
		// Arrange
		var bookingRepo = new Mock<IBookingRepository>();
		var rejseRepo = new Mock<IRejseRepository>();
		var userRepo = new Mock<IUserRepository>();
		var logger = new Mock<ILogger<BookingService>>();

		var booking = Booking.Restore(
			bookingId: 1,
			rejseId: 10,
			userId: 123,
			bookingReference: "BP-TEST123",
			kundeNavn: "Test User",
			kundeEmail: "test@test.dk",
			antalPladser: 2,
			totalPrice: 500m,
			status: BookingStatus.Paid,
			createdAt: DateTime.UtcNow.AddDays(-1),
			paidAt: DateTime.UtcNow.AddDays(-1),
			stripeSessionId: "sess_123",
			stripePaymentIntentId: "pi_123"
		);

		var rejse = CreateValidRejse(rejseId: 10);
		rejseRepo.Setup(x => x.GetById(10)).Returns(rejse);

		bookingRepo.Setup(x => x.GetById(1)).Returns(booking);
		bookingRepo.Setup(x => x.CancelAndReleaseSeats(1)).Returns(true);

		var service = CreateService(bookingRepo, rejseRepo, userRepo, logger);

		// Act
		var result = service.Cancel(1, actingUserId: 123, isStaff: false);

		// Assert
		Assert.True(result);
		bookingRepo.Verify(x => x.GetById(1), Times.Once);
		rejseRepo.Verify(x => x.GetById(10), Times.Once);
		bookingRepo.Verify(x => x.CancelAndReleaseSeats(1), Times.Once);
	}

	[Fact]
	public void Cancel_WhenNonStaffAndWrongUser_ThrowsForbiddenException()
	{
		// Arrange
		var bookingRepo = new Mock<IBookingRepository>();
		var rejseRepo = new Mock<IRejseRepository>();
		var userRepo = new Mock<IUserRepository>();
		var logger = new Mock<ILogger<BookingService>>();

		var booking = Booking.Restore(
			bookingId: 1,
			rejseId: 10,
			userId: 123,
			bookingReference: "BP-TEST123",
			kundeNavn: "Test User",
			kundeEmail: "test@test.dk",
			antalPladser: 2,
			totalPrice: 500m,
			status: BookingStatus.Paid,
			createdAt: DateTime.UtcNow.AddDays(-1),
			paidAt: DateTime.UtcNow.AddDays(-1),
			stripeSessionId: "sess_123",
			stripePaymentIntentId: "pi_123"
		);

		bookingRepo.Setup(x => x.GetById(1)).Returns(booking);

		var service = CreateService(bookingRepo, rejseRepo, userRepo, logger);

		// Act + Assert
		Assert.Throws<ForbiddenException>(() => service.Cancel(1, actingUserId: 999, isStaff: false));

		bookingRepo.Verify(x => x.GetById(1), Times.Once);
		bookingRepo.Verify(x => x.CancelAndReleaseSeats(It.IsAny<int>()), Times.Never);
	}

	[Fact]
	public void Reactivate_WhenPossible_CallsRepository()
	{
		// Arrange
		var bookingRepo = new Mock<IBookingRepository>();
		var rejseRepo = new Mock<IRejseRepository>();
		var userRepo = new Mock<IUserRepository>();
		var logger = new Mock<ILogger<BookingService>>();

		var booking = Booking.Restore(
			1,
			10,
			null,
			"BP-TEST",
			"Test",
			"test@test.dk",
			2,
			200,
			BookingStatus.Cancelled,
			DateTime.UtcNow.AddDays(-1),
			DateTime.UtcNow.AddDays(-1),
			"sess_123",
			"pi_123"
		);

		bookingRepo.Setup(x => x.GetById(1)).Returns(booking);
		bookingRepo.Setup(x => x.ReactivateAndReserveSeats(1)).Returns(true);

		var service = CreateService(bookingRepo, rejseRepo, userRepo, logger);

		// Act
		var result = service.Reactivate(1);

		// Assert
		Assert.True(result);
		bookingRepo.Verify(x => x.GetById(1), Times.Once);
		bookingRepo.Verify(x => x.ReactivateAndReserveSeats(1), Times.Once);
	}

	[Fact]
	public void Reactivate_WhenBookingDoesNotExist_ThrowsNotFoundException()
	{
		// Arrange
		var bookingRepo = new Mock<IBookingRepository>();
		var rejseRepo = new Mock<IRejseRepository>();
		var userRepo = new Mock<IUserRepository>();
		var logger = new Mock<ILogger<BookingService>>();

		bookingRepo.Setup(x => x.GetById(999)).Returns((Booking?)null);

		var service = CreateService(bookingRepo, rejseRepo, userRepo, logger);

		// Act + Assert
		Assert.Throws<NotFoundException>(() => service.Reactivate(999));

		bookingRepo.Verify(x => x.GetById(999), Times.Once);
		bookingRepo.Verify(x => x.ReactivateAndReserveSeats(It.IsAny<int>()), Times.Never);
	}

	[Fact]
	public void Reactivate_WhenNoSeatsAvailable_ThrowsValidationException()
	{
		// Arrange
		var bookingRepo = new Mock<IBookingRepository>();
		var rejseRepo = new Mock<IRejseRepository>();
		var userRepo = new Mock<IUserRepository>();
		var logger = new Mock<ILogger<BookingService>>();

		var booking = Booking.Restore(
			1,
			10,
			null,
			"BP-TEST",
			"Test",
			"test@test.dk",
			2,
			200,
			BookingStatus.Cancelled,
			DateTime.UtcNow.AddDays(-1),
			DateTime.UtcNow.AddDays(-1),
			"sess_123",
			"pi_123"
		);

		bookingRepo.Setup(x => x.GetById(1)).Returns(booking);
		bookingRepo.Setup(x => x.ReactivateAndReserveSeats(1)).Returns(false);

		var service = CreateService(bookingRepo, rejseRepo, userRepo, logger);

		// Act + Assert
		Assert.Throws<ConflictException>(() => service.Reactivate(1));

		bookingRepo.Verify(x => x.GetById(1), Times.Once);
		bookingRepo.Verify(x => x.ReactivateAndReserveSeats(1), Times.Once);
	}
}