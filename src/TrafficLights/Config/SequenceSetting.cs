namespace TrafficLights.Config;

public class SequenceSetting
{
    public int Order { get; init; } = int.MinValue;
    public string Name { get; init; } = string.Empty;
    public string DurationName { get; init; } = string.Empty;
    public IDictionary<string, bool> ActiveLights { get; init; } = new Dictionary<string, bool>();
}