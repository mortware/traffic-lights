﻿namespace TrafficLights.Traffic;

public class Light
{
    public int Id { get; }
    
    public string Key { get; }
    public State State { get; set; }

    public Light(int id, string key)
    {
        Id = id;
        Key = key;
    }
}

public enum State
{
    Red,
    Amber,
    Green
}