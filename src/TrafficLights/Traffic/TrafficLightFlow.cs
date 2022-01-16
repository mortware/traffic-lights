namespace TrafficLights.Traffic;

public class TrafficLightState
{
    public string Name { get; init; }

    public int Duration { get; set; }
    
    public bool SouthToNorthActive { get; init; }
    
    public bool NorthToSouthActive { get; init; }
    
    public bool SouthToEastActive { get; init; }
    
    public bool EastToWestActive { get; init; }
    
    public bool WestToEastActive { get; init; }

    public TrafficLightState(int duration, string name)
    {
        Duration = duration;
        Name = name;
    }
}