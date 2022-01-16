namespace TrafficLights.Config;

public class SequenceSetting
{
    public int Order { get; set; }
    public string Name { get; set; }
    public string DurationName { get; set; }
    public Dictionary<string, bool> ActiveLights { get; set; } = new();
}