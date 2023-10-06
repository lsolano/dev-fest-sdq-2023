namespace AirBnB.DevFest23.Domain;

public interface ICommand<in TArgs, out TOutput>
{
    TOutput Execute(TArgs args);
}
