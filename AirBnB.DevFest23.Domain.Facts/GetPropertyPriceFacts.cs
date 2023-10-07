using AirBnB.DevFest23.Domain.Infrastructure;
using AirBnB.DevFest23.Domain.Models;
using AirBnB.DevFest23.Domain.Queries;
using Moq;
using Optional;
using Optional.Unsafe;

namespace AirBnB.DevFest23.Domain.Facts;

public static class GetPropertyPriceFacts
{
    [TestFixture]
    public sealed class ExecuteFacts
    {
        /*
        Decision Tables
        |-----------------------------------------------------|
        |            |                    | R1 | R2 | R3 | R4 |
        |------------|--------------------|----|----|----|----|
        | Conditions | Hits Holydays?     | N  |  N |  Y |  Y |
        |------------|--------------------|----|----|----|----|
        |            | Zone >= 80% Booked | N  |  Y |  N |  Y |
        |++++++++++++|+++++++++++++++++++++++++|++++|++++|++++|
        | Actions    | +5% Price Inc.     | N  |  Y |  Y |  N |
        |------------|--------------------|----|----|----|----|
        |            | +15% Price Inc.    | N  |  N |  N |  Y |
        |------------|--------------------|----|----|----|----|
        |            | Regular Price      | Y  |  N |  N |  N |
        |-----------------------------------------------------|
        */

        [TestCase(false, false, ExpectedResult = 100)]
        [TestCase(false, true, ExpectedResult = 105)]
        [TestCase(true, false, ExpectedResult = 105)]
        [TestCase(true, true, ExpectedResult = 115)]
        public decimal Returns_Expected_Price(bool hitsHolydays, bool zoneAboveThreshold)
        {
            Guid propertyId = Guid.NewGuid();
            Option<PropertyCapacityInfo> capacityInfoOption = Option.Some(new PropertyCapacityInfo
            {
                Id = Guid.NewGuid(),
                MaxGuests = 9,
                Price = 100
            });

            var propertyRepoMock = new Mock<IPropertyRepository>();
            propertyRepoMock.Setup(m => m.Find(It.IsAny<Guid>()))
                                  .Returns(capacityInfoOption);

            IQuery<GetPropertyPriceArgs, Option<decimal>> sut = new GetPropertyPrice(propertyRepoMock.Object);

            return sut.Execute(new GetPropertyPriceArgs
            {
                PropertyId = propertyId,
                HitsHoliday = hitsHolydays,
                ZoneAboveThreshold = zoneAboveThreshold
            }).ValueOrDefault();
        }
    }
}
