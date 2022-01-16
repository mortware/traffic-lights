namespace TrafficLights.Providers;

public class DateTimeProvider : IDateTimeProvider
{
    public virtual DateTime Now => DateTime.Now;
    public virtual DateTime UtcNow => DateTime.UtcNow;
}

public interface IDateTimeProvider
{
    DateTime Now { get; }
    DateTime UtcNow { get; }
}