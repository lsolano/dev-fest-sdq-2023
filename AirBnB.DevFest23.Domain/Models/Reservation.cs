namespace AirBnB.DevFest23.Domain.Models;

public sealed record class Reservation
{
    public required Guid PropertyId { get; init; }
    public required int Guests { get; init; }
    public required DateOnly CheckIn { get; init; }
    public required DateOnly CheckOut { get; init; }
}
