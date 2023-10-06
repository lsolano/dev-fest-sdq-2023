namespace AirBnB.DevFest23.Domain;

public interface IQuery<in TArgs, out TOutput>
{
    TOutput Execute(TArgs args);
}
