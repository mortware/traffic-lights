using Microsoft.Extensions.Options;
using TrafficLights.Config;

namespace TrafficLights.Traffic;

public class SequenceManager : ISequenceManager
{
    private readonly ILogger<SequenceManager> _logger;
    private readonly TrafficLightSettings _trafficLightSettings;
    public Queue<TrafficLightFlow> DefaultSequence { get; }
    public Queue<TrafficLightFlow> PeakSequence { get; }
    public Queue<TrafficLightFlow> CurrentSequence { get; private set; }

    public SequenceManager(ILogger<SequenceManager> logger, IOptions<TrafficLightSettings> trafficLightSettings)
    {
        _logger = logger;
        _trafficLightSettings = trafficLightSettings.Value;
        DefaultSequence = BuildSequence(_trafficLightSettings.DefaultSequence);
        PeakSequence = BuildSequence(_trafficLightSettings.PeakSequence);
    }

    public TrafficLightFlow GetNextFlow(TimeSpan currentTime)
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

    private Queue<TrafficLightFlow> BuildSequence(IEnumerable<SequenceSetting> sequenceSettings)
    {
        var sequence = new Queue<TrafficLightFlow>();
        foreach (var sequenceSetting in sequenceSettings.OrderBy(s => s.Order))
        {
            if (!_trafficLightSettings.Durations.ContainsKey(sequenceSetting.DurationName))
            {
                throw new ArgumentException($"Duration '{sequenceSetting.DurationName}' does not exist in settings.");
            }

            var duration = _trafficLightSettings.Durations[sequenceSetting.DurationName];

            var state = new TrafficLightFlow(duration, sequenceSetting.Name)
            {
                NorthToSouthActive = sequenceSetting.ActiveLights.ContainsKey(nameof(TrafficLightFlow.NorthToSouthActive)) &&
                                     sequenceSetting.ActiveLights[nameof(TrafficLightFlow.NorthToSouthActive)],

                SouthToNorthActive = sequenceSetting.ActiveLights.ContainsKey(nameof(TrafficLightFlow.SouthToNorthActive)) &&
                                     sequenceSetting.ActiveLights[nameof(TrafficLightFlow.SouthToNorthActive)],
                
                SouthToEastActive = sequenceSetting.ActiveLights.ContainsKey(nameof(TrafficLightFlow.SouthToEastActive)) &&
                                    sequenceSetting.ActiveLights[nameof(TrafficLightFlow.SouthToEastActive)],
                
                EastToWestActive = sequenceSetting.ActiveLights.ContainsKey(nameof(TrafficLightFlow.EastToWestActive)) &&
                                   sequenceSetting.ActiveLights[nameof(TrafficLightFlow.EastToWestActive)],
                
                WestToEastActive = sequenceSetting.ActiveLights.ContainsKey(nameof(TrafficLightFlow.WestToEastActive)) &&
                                   sequenceSetting.ActiveLights[nameof(TrafficLightFlow.WestToEastActive)],
            };
            sequence.Enqueue(state);
        }

        return sequence;
    }
}