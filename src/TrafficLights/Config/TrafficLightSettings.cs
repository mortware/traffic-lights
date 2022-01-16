namespace TrafficLights.Config;

public class TrafficLightSettings
{
    /// <summary>
    /// The delay in milliseconds between evaluating the sequence, and updates to clients
    /// </summary>
    public int TickDelay { get; init; } = 1000;

    /// <summary>
    /// Represents peak times and durations to switch to PeakSequence
    /// </summary>
    public IDictionary<string, PeakTimeSetting> PeakTimes { get; init; } = new Dictionary<string, PeakTimeSetting>();

    /// <summary>
    /// Sequence to run during normal hours
    /// </summary>
    public SequenceSetting[] DefaultSequence { get; init; } = Array.Empty<SequenceSetting>();

    /// <summary>
    /// Sequence to run during Peak Times
    /// </summary>
    public SequenceSetting[] PeakSequence { get; init; } = Array.Empty<SequenceSetting>();

    /// <summary>
    /// Default duration settings in milliseconds
    /// </summary>
    public IDictionary<string, int> DefaultDurations { get; init; } = new Dictionary<string, int>();
}