using Microsoft.AspNetCore.SignalR;
using TrafficLights.Traffic;

namespace TrafficLights.Hubs
{
    public class TrafficHub : Hub<ITraffic>
    {
        
    }

    public interface ITraffic
    {
        Task ShowTime(TimeSpan currentTime, string schedule);

        Task SetLight(string key, string state);
    }
}