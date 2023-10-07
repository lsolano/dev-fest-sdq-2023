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


public sealed class RegisterReservation : ICommand<RegisterReservationArgs, Option<Guid, string>>
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

    public Option<Guid, string> Execute(RegisterReservationArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);

        Option<PropertyCapacityInfo, string> propertyInfoOption
            = args.PropertyId.SomeWhen(id => Guid.Empty != id, $"Invalid PropertyId '{args.PropertyId}'.")
                             .FlatMap(notEmptyId => _propertyRepository.Find(notEmptyId), $"Property '{args.PropertyId}' not found.");

        return propertyInfoOption.FlatMap(info => ValidatePropertyRules(info, args)
                                                  .Map(DoExecute(args)));
    }

    private static Option<PropertyCapacityInfo, string> ValidatePropertyRules(PropertyCapacityInfo info, RegisterReservationArgs args)
    {
        return info.SomeWhen(i => 1 <= args.TotalGuests && args.TotalGuests <= i.MaxGuests,
                             exception: $"Invalid number of guests, must be between 1 and {info.MaxGuests}.")
                    .FlatMap(pi => pi.SomeWhen(_ => pi.Status == PropertyAvailability.Available,
                                               exception: "Unable to create reservation for booked property."));
    }

    private Func<PropertyCapacityInfo, Guid> DoExecute(RegisterReservationArgs args)
        => propInfo =>
            {
                _propertyRepository.UpdateStatus(propInfo.Id, PropertyAvailability.Booked);

                return _reservationRepository.Persist(new()
                {
                    PropertyId = propInfo.Id,
                    Guests = args.TotalGuests,
                    CheckIn = args.CheckInDate,
                    CheckOut = args.CheckOutDate
                });
            };
}
