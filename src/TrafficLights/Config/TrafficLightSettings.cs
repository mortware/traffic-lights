namespace TrafficLights.Config;

public class TrafficLightSettings
{
    /// <summary>
    /// The delay in milliseconds between evaluating the sequence, and updates to clients
    /// </summary>
    public int TickDelay { get; set; }
    
    /// <summary>
    /// Represents peak times and durations to switch to PeakSequence
    /// </summary>
    public Dictionary<string, PeakTimeSetting> PeakTimes { get; set; }
    
    /// <summary>
    /// Sequence to run during normal hours
    /// </summary>
    public IEnumerable<SequenceSetting> DefaultSequence { get; set; }
    
    /// <summary>
    /// Sequence to run during Peak Times
    /// </summary>
    public IEnumerable<SequenceSetting> PeakSequence { get; set; }
    
    /// <summary>
    /// Default duration settings
    /// </summary>
    public Dictionary<string, int> Durations { get; set; }
}