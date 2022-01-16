using TrafficLights.Models;

namespace TrafficLights.Services;

public interface ISequenceService
{
    Queue<TrafficLightFlow> CurrentSequence { get; }
    TrafficLightFlow GetNextFlow(TimeSpan currentTime);
    bool IsPeakTime(TimeSpan currentTime);
}