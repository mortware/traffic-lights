namespace TrafficLights.Providers;

public interface IDateTimeProvider
{
    DateTime Now { get; }
}