using AirBnB.DevFest23.Domain.Models;

namespace AirBnB.DevFest23.Domain.Infrastructure;

public interface IPropertyRepository
{
    PropertyCapacityInfo Find(Guid id);
}
