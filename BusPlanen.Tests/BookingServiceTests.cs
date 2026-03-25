using Moq;
using Xunit;
using BusRejser.Services;
using BusRejserLibrary.Repositories;

namespace BusPlanen.Tests;

public class BookingServiceTests
{
	[Fact]
	public async Task CancelAsync_ShouldCallRepositoryTransactionMethod()
	{
		// Arrange
		var bookingRepository = new Mock<IBookingRepository>();

		var service = new BookingService(
			bookingRepository.Object
		);

		var bookingId = 1;
		var actorUserId = 123;

		// Act
		await service.CancelAsync(bookingId, actorUserId);

		// Assert
		bookingRepository.Verify(
			x => x.CancelAndReleaseSeats(bookingId),
			Times.Once);
	}
}