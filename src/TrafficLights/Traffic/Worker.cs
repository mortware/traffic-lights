using Microsoft.AspNetCore.SignalR;
using TrafficLights.Hubs;

namespace TrafficLights.Traffic;

public class Worker : BackgroundService
{
    private readonly IHubContext<TrafficHub, ITraffic> _hubContext;
    private readonly DateTime _startTime;

    private PeakTime[] PeakTimes = new[]
    {
        new PeakTime { Start = TimeSpan.FromHours(8), Duration = TimeSpan.FromHours(2) },
        new PeakTime { Start = TimeSpan.FromHours(17), Duration = TimeSpan.FromHours(2) },
    };

    private const int DefaultActiveDuration = 5; //20
    private const int DefaultAltDuration = 2; // 4
    private const int DefaultRestDuration = 2; // 4
    private const int DefaultWarningDuration = 2; // 4
    private int _elapsedSeconds = 0;
    private bool _isTransitioning = false;

    private TrafficLightState _currentFlow;


    private readonly Queue<TrafficLightState> _sequence = new();

    public Worker(IHubContext<TrafficHub, ITraffic> hubContextContext)
    {
        _hubContext = hubContextContext;
        _startTime = DateTime.UtcNow;

        // Order of traffic flow
        _sequence.Enqueue(new TrafficLightState("North <-> South") { SouthToNorthActive = true, NorthToSouthActive = true });
        _sequence.Enqueue(new TrafficLightState("South -> North + East") { SouthToNorthActive = true, SouthToEastActive = true });
        _sequence.Enqueue(new TrafficLightState("East <-> West") { EastToWestActive = true, WestToEastActive = true });

        MoveToNextFlow();
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await DoLoop(cancellationToken);
        }
    }

    private async Task DoLoop(CancellationToken cancellationToken)
    {
        await _hubContext.Clients.All.ShowTime(DateTime.UtcNow.TimeOfDay);

        Console.WriteLine(_elapsedSeconds);

        if (!_isTransitioning && _elapsedSeconds >= DefaultActiveDuration - DefaultWarningDuration)
        {
            Console.WriteLine($"Switching from {_currentFlow.Name} to {_sequence.Peek().Name}");
            _isTransitioning = true;
        }
        else if (_elapsedSeconds >= DefaultActiveDuration)
        {
            MoveToNextFlow();
            Console.WriteLine($"Switched to {_currentFlow.Name}");
            _isTransitioning = false;
            _elapsedSeconds = 0;
        }

        _elapsedSeconds++;

        var status = TrafficLightStatus.Build(_currentFlow, _sequence.Peek(), _isTransitioning);

        foreach (var light in status.All)
        {
            await _hubContext.Clients.All.SetLight(light.Key, light.State.ToString().ToLower());
        }

        await Task.Delay(1000, cancellationToken);
    }

    private void MoveToNextFlow()
    {
        _currentFlow = _sequence.Dequeue();
        _sequence.Enqueue(_currentFlow);
    }
}

public class PeakTime
{
    public TimeSpan Start { get; set; }
    public TimeSpan Duration { get; set; }
}

public class TrafficLightState
{
    public string Name { get; init; }
    public bool SouthToNorthActive { get; init; }
    public bool NorthToSouthActive { get; init; }
    public bool SouthToEastActive { get; init; }
    public bool EastToWestActive { get; init; }
    public bool WestToEastActive { get; init; }

    public TrafficLightState(string name)
    {
        Name = name;
    }
}