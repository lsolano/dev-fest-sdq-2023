using AirBnB.DevFest23.Domain.Models;

namespace AirBnB.DevFest23.Domain.Infrastructure;

public interface IReservationRepository
{
    Guid Persist(Reservation reservation);
}
