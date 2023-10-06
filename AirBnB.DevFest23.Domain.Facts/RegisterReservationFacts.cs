using AirBnB.DevFest23.Domain.Commands;
using AirBnB.DevFest23.Domain.Infrastructure;
using AirBnB.DevFest23.Domain.Models;
using FluentAssertions;
using Moq;
using Optional;
using Optional.Unsafe;

namespace AirBnB.DevFest23.Domain.Facts;

public static class RegisterReservationFacts
{
    internal static readonly RegisterReservationArgs Args = new()
    {
        PropertyId = Guid.NewGuid(),
        TotalGuests = 4,
        CheckInDate = DateOnly.Parse("2023-10-15"),
        CheckOutDate = DateOnly.Parse("2023-10-20")
    };
    /*
    Test Plan: 
        Positive
            - Returns generated ID (already there)

        Negative
            - Property Not Found               => Option.None<Guid> (DONE!)
            - Empty Property Id (GUID default), Skips Infra Layer => Option.None<Guid> (DONE!)
            - Null args                        => Throws ArgumentNullException (DONE!)

        Negative DI / Config issues
            - Null repositories => Throws ArgumentNullException (1 test per repo) (DONE!)

        (🤔 Try thinking on another Negative Test Case ....)
    */

    [TestFixture]
    public sealed class ConstructorFacts
    {
        [Test]
        public void With_Null_ReservationRepository_Throws_ArgumentNullException()
        {
            var action = () => new RegisterReservation(null!, new Mock<IPropertyRepository>().Object);

            action.Should().Throw<ArgumentNullException>().WithMessage("*reservationRepository*");
        }

        [Test]
        public void With_Null_PropertyRepository_Throws_ArgumentNullException()
        {
            var action = () => new RegisterReservation(new Mock<IReservationRepository>().Object, null!);

            action.Should().Throw<ArgumentNullException>().WithMessage("*propertyRepository*");
        }
    }

    [TestFixture]
    public sealed class ExecuteFacts
    {
        [Test]
        public void With_Null_Args_Throws_ArgumentNullException()
        {
            ICommand<RegisterReservationArgs, Option<Guid>> sut
                = new RegisterReservation(new Mock<IReservationRepository>().Object, new Mock<IPropertyRepository>().Object);

            var action = () => sut.Execute(null!);

            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void With_Not_Found_Property_Returns_None()
        {
            var reservationRepoMock = new Mock<IReservationRepository>();
            var propertyRepositoryMock = new Mock<IPropertyRepository>();
            propertyRepositoryMock.Setup(m => m.Find(It.IsAny<Guid>()))
                                  .Returns(Option.None<PropertyCapacityInfo>());

            ICommand<RegisterReservationArgs, Option<Guid>> sut
                = new RegisterReservation(reservationRepoMock.Object, propertyRepositoryMock.Object);

            Option<Guid> reservationId = sut.Execute(Args);

            reservationId.Should().Be(Option.None<Guid>());
        }

        [Test]
        public void With_Empty_Id_On_Args_Skips_Infra_Layer()
        {
            var reservationRepoMock = new Mock<IReservationRepository>();
            var propertyRepositoryMock = new Mock<IPropertyRepository>();
            propertyRepositoryMock.Setup(m => m.Find(It.IsAny<Guid>()))
                                  .Returns(Option.None<PropertyCapacityInfo>());

            ICommand<RegisterReservationArgs, Option<Guid>> sut
                = new RegisterReservation(reservationRepoMock.Object, propertyRepositoryMock.Object);

            Option<Guid> reservationId = sut.Execute(Args with { PropertyId = Guid.Empty });

            reservationId.Should().Be(Option.None<Guid>());
            propertyRepositoryMock.Verify(x => x.Find(It.IsAny<Guid>()), Times.Never);
            reservationRepoMock.Verify(x => x.Persist(It.IsAny<Reservation>()), Times.Never);
        }

        [Test]
        public void With_Valid_Args_Returns_Infrastructure_Reservation_Id()
        {
            Guid expectedReservationId = Guid.NewGuid();

            var reservationRepoMock = new Mock<IReservationRepository>();
            reservationRepoMock.Setup(m => m.Persist(It.IsAny<Reservation>()))
                               .Returns(expectedReservationId);
            IReservationRepository reservationRepository = reservationRepoMock.Object;
            var propertyRepositoryMock = new Mock<IPropertyRepository>();
            propertyRepositoryMock.Setup(m => m.Find(It.IsAny<Guid>()))
                                  .Returns(Option.Some(new PropertyCapacityInfo
                                  {
                                      Id = Guid.NewGuid(),
                                      MaxGuests = 16
                                  }));

            ICommand<RegisterReservationArgs, Option<Guid>> sut
                = new RegisterReservation(reservationRepository, propertyRepositoryMock.Object);

            Option<Guid> reservationId = sut.Execute(new RegisterReservationArgs
            {
                PropertyId = Guid.NewGuid(),
                TotalGuests = 4,
                CheckInDate = DateOnly.Parse("2023-10-15"),
                CheckOutDate = DateOnly.Parse("2023-10-20")
            });

            reservationId.ValueOrDefault().Should().Be(expectedReservationId);
        }
    }

}
