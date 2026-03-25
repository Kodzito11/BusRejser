using BusRejserLibrary.Enums;
using BusRejserLibrary.Models;
using BusRejserLibrary.Repositories;
using BusRejserLibrary.Services;
using BusRejser.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BusPlanen.Tests;

public class BookingServiceTests
{
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

		bookingRepo
			.Setup(x => x.GetById(1))
			.Returns(booking);

		bookingRepo
			.Setup(x => x.CancelAndReleaseSeats(1))
			.Returns(true);

		var service = new BookingService(
			bookingRepo.Object,
			rejseRepo.Object,
			userRepo.Object,
			logger.Object
		);

		// Act
		var result = service.Cancel(1, actingUserId: null, isStaff: true);

		// Assert
		Assert.True(result);

		bookingRepo.Verify(x => x.GetById(1), Times.Once);
		bookingRepo.Verify(x => x.CancelAndReleaseSeats(1), Times.Once);
	}

	[Fact]
	public void Create_WhenSeatsAvailable_CreatesBooking()
	{
		// Arrange
		var bookingRepo = new Mock<IBookingRepository>();
		var rejseRepo = new Mock<IRejseRepository>();
		var userRepo = new Mock<IUserRepository>();
		var logger = new Mock<ILogger<BookingService>>();

		var rejse = Rejse.Create(
			"Test",
			"København",
			DateTime.UtcNow.AddDays(2),
			DateTime.UtcNow.AddDays(3),
			100,
			50,
			null
		);

		rejse.RejseId = 1;
		rejse.BookedSeats = 10;

		rejseRepo
			.Setup(x => x.GetById(1))
			.Returns(rejse);

		rejseRepo
			.Setup(x => x.TryReserveSeats(1, 2))
			.Returns(true);

		bookingRepo
			.Setup(x => x.Create(It.IsAny<Booking>()))
			.Returns(1);

		var service = new BookingService(
			bookingRepo.Object,
			rejseRepo.Object,
			userRepo.Object,
			logger.Object
		);

		var booking = Booking.Create(
			1,
			null,
			"Test",
			"test@test.dk",
			2,
			200
		);

		booking.MarkAsPaid("sess", "pi");

		// Act
		var id = service.Create(booking);

		// Assert
		Assert.Equal(1, id);

		bookingRepo.Verify(x => x.Create(It.IsAny<Booking>()), Times.Once);
	}

	[Fact]
	public void Create_WhenNoSeatsAvailable_ThrowsValidationException()
	{
		// Arrange
		var bookingRepo = new Mock<IBookingRepository>();
		var rejseRepo = new Mock<IRejseRepository>();
		var userRepo = new Mock<IUserRepository>();
		var logger = new Mock<ILogger<BookingService>>();

		var rejse = Rejse.Create(
			"Test",
			"København",
			DateTime.UtcNow.AddDays(2),
			DateTime.UtcNow.AddDays(3),
			100,
			10,
			null
		);

		rejse.RejseId = 1;

		rejseRepo
			.Setup(x => x.GetById(1))
			.Returns(rejse);

		rejseRepo
			.Setup(x => x.TryReserveSeats(1, 5))
			.Returns(false);

		var service = new BookingService(
			bookingRepo.Object,
			rejseRepo.Object,
			userRepo.Object,
			logger.Object
		);

		var booking = Booking.Create(
			1,
			null,
			"Test",
			"test@test.dk",
			5,
			500
		);

		booking.MarkAsPaid("sess", "pi");

		// Act + Assert
		Assert.Throws<ValidationException>(() => service.Create(booking));
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
			DateTime.UtcNow,
			null,
			null,
			null
		);

		bookingRepo.Setup(x => x.GetById(1)).Returns(booking);
		bookingRepo.Setup(x => x.ReactivateAndReserveSeats(1)).Returns(true);

		var service = new BookingService(
			bookingRepo.Object,
			rejseRepo.Object,
			userRepo.Object,
			logger.Object
		);

		// Act
		var result = service.Reactivate(1);

		// Assert
		Assert.True(result);
		bookingRepo.Verify(x => x.ReactivateAndReserveSeats(1), Times.Once);
	}

	[Fact]
	public void Create_RejseWhenDoestNotExist_ThrowsNotFoundException()
	{
		// Arrange
		var bookingRepo = new Mock<IBookingRepository>();
		var rejseRepo = new Mock<IRejseRepository>();
		var userRepo = new Mock<IUserRepository>();
		var logger = new Mock<ILogger<BookingService>>();

		rejseRepo.Setup(x => x.GetById(999)).Returns((Rejse?)null);

		var service = new BookingService(
			bookingRepo.Object,
			rejseRepo.Object,
			userRepo.Object,
			logger.Object
		);

		var booking = Booking.Create(
			999,
			null,
			"Test User",
			"test@test.dk",
			2,
			200m
		);

		booking.MarkAsPaid("sess_123", "pi_123");

		//Act + Assert
		Assert.Throws<NotFoundException>(() => service.Create(booking));

	}

	[Fact]
	public void Create_WhenBookingIsNotPaid_ThrowsValidationException()
	{
		// Arrange
		var bookingRepo = new Mock<IBookingRepository>();
		var rejseRepo = new Mock<IRejseRepository>();
		var userRepo = new Mock<IUserRepository>();
		var logger = new Mock<ILogger<BookingService>>();

		var rejse = Rejse.Create(
			"Test",
			"København",
			DateTime.UtcNow.AddDays(2),
			DateTime.UtcNow.AddDays(3),
			100,
			50,
			null
		);

		rejse.RejseId = 1;
		rejseRepo
			.Setup(x => x.GetById(1))
			.Returns(rejse);


		var service = new BookingService(
			bookingRepo.Object,
			rejseRepo.Object,
			userRepo.Object,
			logger.Object
		);

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
		bookingRepo.Verify(x => x.Create(It.IsAny<Booking>()), Times.Never);
	}

	[Fact]
	public void Create_WhenRepositoryFailsAfterSeatReservation_ReleasesSeatsAndRethrows()
	{
		

		// Arrange
		var bookingRepo = new Mock<IBookingRepository>();
		var rejseRepo = new Mock<IRejseRepository>();
		var userRepo = new Mock<IUserRepository>();
		var logger = new Mock<ILogger<BookingService>>();

		var rejse = Rejse.Create(
			"Test",
			"København",
			DateTime.UtcNow.AddDays(2),
			DateTime.UtcNow.AddDays(3),
			100,
			50,
			null
		);

		rejse.RejseId = 1;

		rejseRepo
			.Setup(x => x.GetById(1))
			.Returns(rejse);

		rejseRepo
			.Setup(x => x.TryReserveSeats(1, 2))
			.Returns(true);

		bookingRepo
			.Setup(x => x.Create(It.IsAny<Booking>()))
			.Throws(new Exception("DB failed"));

		var service = new BookingService(
			bookingRepo.Object,
			rejseRepo.Object,
			userRepo.Object,
			logger.Object
		);

		var booking = Booking.Create(
			1,
			null,
			"Test User",
			"test@test.dk",
			2,
			200m
		);
		
		booking.MarkAsPaid("sess_123", "pi_123");
		

		// Act + Assert
		var ex = Assert.Throws<Exception>(() => service.Create(booking));

		Assert.Equal("DB failed", ex.Message);
		rejseRepo.Verify(x => x.TryReserveSeats(1, 2), Times.Once);
		rejseRepo.Verify(x => x.ReleaseSeats(1, 2), Times.Once);
		bookingRepo.Verify(x => x.Create(It.IsAny<Booking>()), Times.Once);
	}
}