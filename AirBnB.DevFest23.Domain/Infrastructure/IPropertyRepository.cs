using AirBnB.DevFest23.Domain.Models;
using Optional;

namespace AirBnB.DevFest23.Domain.Infrastructure;

public interface IPropertyRepository
{
    Option<PropertyCapacityInfo> Find(Guid id);
}
