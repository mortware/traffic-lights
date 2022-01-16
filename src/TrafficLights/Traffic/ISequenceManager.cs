namespace TrafficLights.Traffic;

public interface ISequenceManager
{
    Queue<TrafficLightFlow> DefaultSequence { get; }
    Queue<TrafficLightFlow> PeakSequence { get; }
    Queue<TrafficLightFlow> CurrentSequence { get; }
    TrafficLightFlow GetNextFlow(TimeSpan currentTime);
    bool IsPeakTime(TimeSpan currentTime);
}