using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using TrafficLights.Config;
using TrafficLights.Hubs;
using TrafficLights.Providers;

namespace TrafficLights.Traffic;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IHubContext<TrafficHub, ITraffic> _hubContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ISequenceManager _sequenceManager;
    
    private int _elapsedSeconds = 0;
    private bool _isTransitioning = false;
    private readonly int _defaultWarningDuration;
    private int _tickDelay = 1000;
    private TrafficLightState _currentState;

    public Worker(ILogger<Worker> logger, IHubContext<TrafficHub, ITraffic> hubContextContext, IDateTimeProvider dateTimeProvider, ISequenceManager sequenceManager, IOptions<TrafficLightSettings> trafficLightSettings)
    {
        _logger = logger;
        _hubContext = hubContextContext;
        _dateTimeProvider = dateTimeProvider;
        _sequenceManager = sequenceManager;
        _defaultWarningDuration = trafficLightSettings.Value.Durations["DefaultWarningDuration"];
        _tickDelay = trafficLightSettings.Value.TickDelay;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _currentState = _sequenceManager.GetNextFlow(_dateTimeProvider.Now.TimeOfDay);
        while (!cancellationToken.IsCancellationRequested)
        {
            await _hubContext.Clients.All.ShowTime(_dateTimeProvider.Now.TimeOfDay, _sequenceManager.IsPeakTime(_dateTimeProvider.Now.TimeOfDay) ? "Peak" : "Normal");
            await DoLoop(cancellationToken);
        }
    }

    private async Task DoLoop(CancellationToken cancellationToken)
    {
        if (!_isTransitioning && _elapsedSeconds >= _currentState.Duration - _defaultWarningDuration)
        {
            _logger.LogInformation($"Switching from {_currentState.Name} to {_sequenceManager.CurrentSequence.Peek().Name}");
            _isTransitioning = true;
        }
        else if (_elapsedSeconds >= _currentState.Duration)
        {
            _currentState = _sequenceManager.GetNextFlow(_dateTimeProvider.Now.TimeOfDay);
            _logger.LogInformation($"Switched to {_currentState.Name}");
            _isTransitioning = false;
            _elapsedSeconds = 0;
        }
        var status = TrafficLightStatusProvider.Build(_currentState, _sequenceManager.CurrentSequence.Peek(), _isTransitioning);
        await UpdateLights(status);
        
        _elapsedSeconds++;
        await Task.Delay(_tickDelay, cancellationToken);
    }

    private async Task UpdateLights(TrafficLightStatusProvider statusProvider)
    {
        foreach (var light in statusProvider.All)
        {
            await _hubContext.Clients.All.SetLight(light.Key, light.State.ToString().ToLower());
        }
    }
}

