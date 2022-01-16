namespace TrafficLights.Hubs;

public interface ITraffic
{
    Task UpdateTime(TimeSpan currentTime, string schedule);

    Task UpdateCurrentFlowInfo(string currentFlowName, string nextFlowName);

    Task UpdateTrafficLight(string key, string state);
}