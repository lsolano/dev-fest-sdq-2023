using AirBnB.DevFest23.Domain.Infrastructure;
using AirBnB.DevFest23.Domain.Models;
using Optional;

namespace AirBnB.DevFest23.Domain.Commands;

public sealed record class RegisterReservationArgs
{
    public required Guid PropertyId { get; init; }

    public required int TotalGuests { get; init; }

    public required DateOnly CheckInDate { get; init; }
    public required DateOnly CheckOutDate { get; init; }
}


public sealed class RegisterReservation : ICommand<RegisterReservationArgs, Option<Guid>>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IPropertyRepository _propertyRepository;

    public RegisterReservation(
        IReservationRepository reservationRepository,
        IPropertyRepository propertyRepository)
    {
        ArgumentNullException.ThrowIfNull(reservationRepository);
        ArgumentNullException.ThrowIfNull(propertyRepository);

        _reservationRepository = reservationRepository;
        _propertyRepository = propertyRepository;
    }

    public Option<Guid> Execute(RegisterReservationArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);

        Option<PropertyCapacityInfo> propertyInfoOption
            = args.PropertyId.SomeWhen(id => Guid.Empty != id)
                             .FlatMap(notEmptyId => _propertyRepository.Find(notEmptyId));

        return propertyInfoOption.Map(propInfo => _reservationRepository.Persist(new()
        {
            PropertyId = propInfo.Id,
            Guests = args.TotalGuests,
            CheckIn = args.CheckInDate,
            CheckOut = args.CheckOutDate
        }));
    }
}
