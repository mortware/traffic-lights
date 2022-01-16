using Microsoft.AspNetCore.SignalR;
using TrafficLights.Traffic;

namespace TrafficLights.Hubs
{
    public class TrafficHub : Hub<ITraffic>
    {
        
    }

    public interface ITraffic
    {
        Task UpdateTime(TimeSpan currentTime, string schedule);
        
        Task UpdateCurrentFlowInfo(string currentFlowName, string nextFlowName);

        Task UpdateTrafficLight(string key, string state);
    }
}