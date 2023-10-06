using AirBnB.DevFest23.Domain.Infrastructure;
using AirBnB.DevFest23.Domain.Models;

namespace AirBnB.DevFest23.Domain.Commands;

public sealed record class RegisterReservationArgs
{
    public required Guid PropertyId { get; init; }

    public required int TotalGuests { get; init; }

    public required DateOnly CheckInDate { get; init; }
    public required DateOnly CheckOutDate { get; init; }
}


public sealed class RegisterReservation : ICommand<RegisterReservationArgs, Guid>
{
    private readonly IReservationRepository _reservationRepository;

    public RegisterReservation(
        IReservationRepository reservationRepository)
    {
        _reservationRepository = reservationRepository;
    }
    
    public Guid Execute(RegisterReservationArgs args)
    {
        return _reservationRepository.Persist(new Reservation
        {
            PropertyId = args.PropertyId,
            Guests = args.TotalGuests,
            CheckIn = args.CheckInDate,
            CheckOut = args.CheckOutDate
        });
    }
}
