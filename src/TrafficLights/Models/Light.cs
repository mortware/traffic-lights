namespace TrafficLights.Models;

public class Light
{
    public string Key { get; }
    public TrafficLightState State { get; set; }

    public Light(string key)
    {
        Key = key;
    }
}

