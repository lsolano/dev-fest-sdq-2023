using AirBnB.DevFest23.Domain.Infrastructure;
using Optional;

namespace AirBnB.DevFest23.Domain.Queries;

public sealed record class GetPropertyPriceArgs
{
    public required Guid PropertyId { get; init; }
    public required bool HitsHoliday { get; init; }
    public required bool ZoneAboveThreshold { get; init; }
}

public sealed class GetPropertyPrice : IQuery<GetPropertyPriceArgs, Option<decimal>>
{
    private readonly IPropertyRepository _propertyRepository;

    public GetPropertyPrice(IPropertyRepository propertyRepository)
    {
        _propertyRepository = propertyRepository;
    }

    public Option<decimal> Execute(GetPropertyPriceArgs args)
    {
        var infoOpt = _propertyRepository.Find(args.PropertyId);

        return infoOpt.Map(propertyInfo =>
        {
            decimal rate = 0;
            if (args.HitsHoliday && args.ZoneAboveThreshold)
                rate = 0.15M;

            if (args.HitsHoliday ^ args.ZoneAboveThreshold)
                rate = 0.05M;

            return propertyInfo.Price * (1 + rate);
        });
    }
}
