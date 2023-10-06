using AirBnB.DevFest23.Domain.Commands;
using AirBnB.DevFest23.Domain.Infrastructure;
using AirBnB.DevFest23.Domain.Models;
using FluentAssertions;
using Moq;

namespace AirBnB.DevFest23.Domain.Facts;

[TestFixture]
public sealed class RegisterReservationFacts
{
    [Test]
    public void Execute_Returns_Infrastructure_Reservation_Id()
    {
        Guid expectedReservationId = Guid.NewGuid();

        var reservationRepoMock = new Mock<IReservationRepository>();
        reservationRepoMock.Setup(m => m.Persist(It.IsAny<Reservation>()))
                           .Returns(expectedReservationId);
        IReservationRepository reservationRepository = reservationRepoMock.Object;

        ICommand<RegisterReservationArgs, Guid> sut = new RegisterReservation(reservationRepository);

        Guid reservationId = sut.Execute(new RegisterReservationArgs
        {
            PropertyId = Guid.NewGuid(),
            TotalGuests = 4,
            CheckInDate = DateOnly.Parse("2023-10-15"),
            CheckOutDate = DateOnly.Parse("2023-10-20")
        });

        reservationId.Should().Be(expectedReservationId);
    }
}
