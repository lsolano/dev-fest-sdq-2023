namespace AirBnB.DevFest23.Domain.Models;

public sealed record class PropertyCapacityInfo
{
    public required Guid Id { get; init; }

    public required int MaxGuests { get; init; }

    public PropertyAvailability Status { get; init; } = PropertyAvailability.Available;
    
    public int Price { get; init; }
}
