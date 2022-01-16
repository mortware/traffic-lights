using Microsoft.Extensions.Options;
using TrafficLights.Config;
using TrafficLights.Models;

namespace TrafficLights.Services;

public class SequenceService : ISequenceService
{
    private readonly ILogger<SequenceService> _logger;
    private readonly TrafficLightSettings _trafficLightSettings;
    private readonly Queue<TrafficLightFlow> _defaultSequence;
    private readonly Queue<TrafficLightFlow> _peakSequence;
    public Queue<TrafficLightFlow> CurrentSequence { get; private set; }

    public SequenceService(ILogger<SequenceService> logger, IOptions<TrafficLightSettings> trafficLightSettings)
    {
        _logger = logger;
        _trafficLightSettings = trafficLightSettings.Value;
        _defaultSequence = BuildSequence(_trafficLightSettings.DefaultSequence);
        _peakSequence = BuildSequence(_trafficLightSettings.PeakSequence);
        CurrentSequence = _defaultSequence;
    }

    public TrafficLightFlow GetNextFlow(TimeSpan currentTime)
    {
        CurrentSequence = IsPeakTime(currentTime)
            ? _peakSequence
            : _defaultSequence;

        var flow = CurrentSequence.Dequeue();
        CurrentSequence.Enqueue(flow);
        
        _logger.LogInformation($"{nameof(GetNextFlow)} returning '{flow.Name}'");
        
        return flow;
    }

    public bool IsPeakTime(TimeSpan currentTime)
    {
        var peakTimes = _trafficLightSettings.PeakTimes.Values;
        return peakTimes.Any(p => currentTime >= p.Start && currentTime < p.Start.Add(p.Duration));
    }

    private Queue<TrafficLightFlow> BuildSequence(SequenceSetting[] sequenceSettings)
    {
        if (!sequenceSettings.Any())
        {
            throw new Exception("Sequence contains no flow configurations");
        }
        
        var sequence = new Queue<TrafficLightFlow>();
        
        foreach (var sequenceSetting in sequenceSettings.OrderBy(s => s.Order))
        {
            if (!_trafficLightSettings.DefaultDurations.ContainsKey(sequenceSetting.DurationName))
            {
                throw new ArgumentException($"Duration '{sequenceSetting.DurationName}' does not exist in settings.");
            }

            var duration = _trafficLightSettings.DefaultDurations[sequenceSetting.DurationName];

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
                                   sequenceSetting.ActiveLights[nameof(TrafficLightFlow.WestToEastActive)]
            };
            sequence.Enqueue(state);
        }

        return sequence;
    }
}