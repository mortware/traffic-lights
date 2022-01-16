using Microsoft.Extensions.Options;
using TrafficLights.Config;

namespace TrafficLights.Traffic;

public class SequenceManager : ISequenceManager
{
    private readonly ILogger<SequenceManager> _logger;
    private readonly TrafficLightSettings _trafficLightSettings;
    public Queue<TrafficLightState> DefaultSequence { get; }
    public Queue<TrafficLightState> PeakSequence { get; }
    public Queue<TrafficLightState> CurrentSequence { get; private set; }

    public SequenceManager(ILogger<SequenceManager> logger, IOptions<TrafficLightSettings> trafficLightSettings)
    {
        _logger = logger;
        _trafficLightSettings = trafficLightSettings.Value;
        DefaultSequence = BuildSequence(_trafficLightSettings.DefaultSequence);
        PeakSequence = BuildSequence(_trafficLightSettings.PeakSequence);
    }
    
    public TrafficLightState GetNextFlow(TimeSpan currentTime)
    {
        _logger.LogInformation($"Fetching next flow...");
        
        CurrentSequence = IsPeakTime(currentTime)
            ? PeakSequence
            : DefaultSequence;

        var flow = CurrentSequence.Dequeue();
        CurrentSequence.Enqueue(flow);
        return flow;
    }
    
    public bool IsPeakTime(TimeSpan currentTime)
    {
        var peakTimes = _trafficLightSettings.PeakTimes.Values;
        return peakTimes.Any(p => currentTime >= p.Start && currentTime < p.Start.Add(p.Duration));
    }

    private Queue<TrafficLightState> BuildSequence(IEnumerable<SequenceSetting> sequenceSettings)
    {
        var sequence = new Queue<TrafficLightState>();
        foreach (var sequenceSetting in sequenceSettings.OrderBy(s => s.Order))
        {
            if (!_trafficLightSettings.Durations.ContainsKey(sequenceSetting.DurationName))
            {
                throw new ArgumentException($"Duration '{sequenceSetting.DurationName}' does not exist in settings.");
            }
            
            var duration = _trafficLightSettings.Durations[sequenceSetting.DurationName];

            var state = new TrafficLightState(duration, sequenceSetting.Name)
            {
                NorthToSouthActive = sequenceSetting.ActiveLights.ContainsKey(nameof(TrafficLightState.NorthToSouthActive)) && sequenceSetting.ActiveLights[nameof(TrafficLightState.NorthToSouthActive)],
                SouthToNorthActive = sequenceSetting.ActiveLights.ContainsKey(nameof(TrafficLightState.SouthToNorthActive)) && sequenceSetting.ActiveLights[nameof(TrafficLightState.SouthToNorthActive)],
                SouthToEastActive = sequenceSetting.ActiveLights.ContainsKey(nameof(TrafficLightState.SouthToEastActive)) && sequenceSetting.ActiveLights[nameof(TrafficLightState.SouthToEastActive)],
                EastToWestActive = sequenceSetting.ActiveLights.ContainsKey(nameof(TrafficLightState.EastToWestActive)) && sequenceSetting.ActiveLights[nameof(TrafficLightState.EastToWestActive)],
                WestToEastActive = sequenceSetting.ActiveLights.ContainsKey(nameof(TrafficLightState.WestToEastActive)) && sequenceSetting.ActiveLights[nameof(TrafficLightState.WestToEastActive)],
            };
            sequence.Enqueue(state);
        }
        return sequence;
    }
}

public interface ISequenceManager
{
    Queue<TrafficLightState> DefaultSequence { get; }
    Queue<TrafficLightState> PeakSequence { get; }
    Queue<TrafficLightState> CurrentSequence { get; }
    TrafficLightState GetNextFlow(TimeSpan currentTime);
    bool IsPeakTime(TimeSpan currentTime);
}