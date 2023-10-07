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
            ICommand<RegisterReservationArgs, Option<Guid, string>> sut
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

            ICommand<RegisterReservationArgs, Option<Guid, string>> sut
                = new RegisterReservation(reservationRepoMock.Object, propertyRepositoryMock.Object);

            Option<Guid, string> reservationId = sut.Execute(Args);

            reservationId.Should().Be(Option.None<Guid, string>($"Property '{Args.PropertyId}' not found."));
        }

        [Test]
        public void With_Empty_Id_On_Args_Skips_Infra_Layer()
        {
            var reservationRepoMock = new Mock<IReservationRepository>();
            var propertyRepositoryMock = new Mock<IPropertyRepository>();
            propertyRepositoryMock.Setup(m => m.Find(It.IsAny<Guid>()))
                                  .Returns(Option.None<PropertyCapacityInfo>());

            ICommand<RegisterReservationArgs, Option<Guid, string>> sut
                = new RegisterReservation(reservationRepoMock.Object, propertyRepositoryMock.Object);

            Option<Guid, string> reservationId = sut.Execute(Args with { PropertyId = Guid.Empty });

            reservationId.Should().Be(Option.None<Guid, string>($"Invalid PropertyId '{Guid.Empty}'."));
            propertyRepositoryMock.Verify(x => x.Find(It.IsAny<Guid>()), Times.Never);
            reservationRepoMock.Verify(x => x.Persist(It.IsAny<Reservation>()), Times.Never);
        }

        /*
        Boundary Value Analysis (BVA) & Equivalence Partitions (EP)
        With Allowed Guests between [1, 9]
            1. BVA 
                1.1 0 Guests => Option.None<Guid, String>("Invalid number of guests, must be between 1 and 9.")
                1.2 1 Guests => Option.Some<Guid, String>(infraLayer.ReservationId)
                1.2 2 Guests => IDEM
                1.3 5 Guests => IDEM
                1.4 8 Guests => IDEM
                1.5 9 Guests => IDEM
                1.6 10 Guests => Option.None<Guid, String>("Invalid number of guests, must be between 1 and 9.")

            2. EP
                2.1 IDEM 1.1
                2.2 IDEM 1.3
                2.3 IDEM 1.6
        */


        [Test] //Cases 1.2 to 1.5
        public void With_9_MaxGuests_And_Allowed_Guests_Count_Creates_Reservation(
            [Values(1, 2, 5, 8, 9)] int guests)
        {
            Guid expectedReservationId = Guid.NewGuid();
            Option<PropertyCapacityInfo> capacityInfoOption = Option.Some(new PropertyCapacityInfo
            {
                Id = Guid.NewGuid(),
                MaxGuests = 9
            });

            ICommand<RegisterReservationArgs, Option<Guid, string>> sut = BuildSutWithSimpleMocks(expectedReservationId, capacityInfoOption);

            Option<Guid, string> reservationId = sut.Execute(Args with { TotalGuests = guests });

            reservationId.ValueOrDefault().Should().Be(expectedReservationId);
        }

        [Test] //Cases 1.1 and 1.6
        public void With_9_MaxGuests_And_Not_Allowed_Guests_Count_Returns_None_With_Error_Msg(
            [Values(0, 10)] int guests)
        {
            Guid expectedReservationId = Guid.NewGuid();
            Option<PropertyCapacityInfo> capacityInfoOption = Option.Some(new PropertyCapacityInfo
            {
                Id = Guid.NewGuid(),
                MaxGuests = 9, 
            });

            ICommand<RegisterReservationArgs, Option<Guid, string>> sut = BuildSutWithSimpleMocks(expectedReservationId, capacityInfoOption);

            Option<Guid, string> reservationId = sut.Execute(Args with { TotalGuests = guests });

            reservationId.MatchSome(id => Assert.Fail($"Expected error message, but found Guid '{id}'"));
            reservationId.MatchNone(errorMessage => errorMessage.Should()
                                                                .Be("Invalid number of guests, must be between 1 and 9."));
        }

        /*
        Trivia: try to add a new test case to force an optimization over Execute()
            -> Do not call Infra Layer if Args.TotalGuests is less than 1
        */

        /*
            State Diagrams
            1. With Available property Works => Updates property status to booked
            2. With Booked property => Option.None<Guid, String>("Unable to create reservation for booked property.")

                (*) ---> (Available) ---- RegisterReservation ---> (Booked)
                            ^                                           |
                            |                                           |
                            |-------------CancelReservation-------------|
        */

        [Test]
        public void With_Booked_Property_Returns_None_And_ErrorMessage()
        {
            Guid expectedReservationId = Guid.NewGuid();
            Option<PropertyCapacityInfo> capacityInfoOption = Option.Some(new PropertyCapacityInfo
            {
                Id = Guid.NewGuid(),
                MaxGuests = 9,
                Status = PropertyAvailability.Booked
            });

            ICommand<RegisterReservationArgs, Option<Guid, string>> sut = BuildSutWithSimpleMocks(expectedReservationId, capacityInfoOption);

            Option<Guid, string> reservationId = sut.Execute(Args);

            reservationId.MatchSome(id => Assert.Fail($"Expected error message, but found Guid '{id}'"));
            reservationId.MatchNone(errorMessage => errorMessage.Should()
                                                                .Be("Unable to create reservation for booked property."));
        }

        [Test]
        public void With_Available_Property_Updates_Its_Status_To_Booked()
        {
            Guid expectedReservationId = Guid.NewGuid();
            Guid propertyId = Guid.NewGuid();

            Option<PropertyCapacityInfo> capacityInfoOption = Option.Some(new PropertyCapacityInfo
            {
                Id = propertyId,
                MaxGuests = 9,
                Status = PropertyAvailability.Available
            });

            var propertyRepoMock = new Mock<IPropertyRepository>();
            ICommand<RegisterReservationArgs, Option<Guid, string>> sut 
                = BuildSutWithSimpleMocks(expectedReservationId, capacityInfoOption, propertyRepoMock: propertyRepoMock);

            _ = sut.Execute(Args with { PropertyId = propertyId });

            propertyRepoMock.Verify(x => x.UpdateStatus(
                It.Is<Guid>(id => id == propertyId), 
                It.Is<PropertyAvailability>(s => s == PropertyAvailability.Booked)), Times.Once);
            
        }

        //Homework: Add the CancelReservation command, be sure to include a guard for the only allowed transition there Booked -> Available

        private static ICommand<RegisterReservationArgs, Option<Guid, string>> BuildSutWithSimpleMocks(
            Guid expectedReservationId, 
            Option<PropertyCapacityInfo> capacityInfoOption,
            Mock<IReservationRepository>? reservationRepoMock = null,
            Mock<IPropertyRepository>? propertyRepoMock = null)
        {
            var finalReservationMock = reservationRepoMock ?? new Mock<IReservationRepository>();
            finalReservationMock.Setup(m => m.Persist(It.IsAny<Reservation>()))
                               .Returns(expectedReservationId);

            var finalPropertyRepositoryMock = propertyRepoMock ?? new Mock<IPropertyRepository>();
            finalPropertyRepositoryMock.Setup(m => m.Find(It.IsAny<Guid>()))
                                  .Returns(capacityInfoOption);

            return new RegisterReservation(finalReservationMock.Object, finalPropertyRepositoryMock.Object);
        }
    }

}
