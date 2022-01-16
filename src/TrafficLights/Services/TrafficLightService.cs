using System.Diagnostics;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using TrafficLights.Config;
using TrafficLights.Hubs;
using TrafficLights.Models;
using TrafficLights.Providers;

namespace TrafficLights.Services;

public class TrafficLightService : BackgroundService
{
    private readonly ILogger<TrafficLightService> _logger;
    private readonly IHubContext<TrafficHub, ITraffic> _hubContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ISequenceService _sequenceService;
    
    private readonly Stopwatch _timer;
    private readonly int _delay;
    private int _elapsed;
    private bool _isTransitioning;

    private readonly int _defaultWarningDuration;
    private TrafficLightFlow _currentFlow;

    public TrafficLightService(
        ILogger<TrafficLightService> logger, 
        IHubContext<TrafficHub, ITraffic> hubContextContext, 
        IDateTimeProvider dateTimeProvider, 
        ISequenceService sequenceService, 
        IOptions<TrafficLightSettings> trafficLightSettings)
    {
        _logger = logger;
        _hubContext = hubContextContext;
        _dateTimeProvider = dateTimeProvider;
        _sequenceService = sequenceService;
        _defaultWarningDuration = trafficLightSettings.Value.DefaultDurations["DefaultWarningDuration"];
        _delay = trafficLightSettings.Value.TickDelay;
        _timer = new Stopwatch();
        _currentFlow = new TrafficLightFlow(int.MaxValue, "Rest");
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await UpdateFlow();
        _logger.LogDebug($"Starting with {_currentFlow.Name}...");
        
        while (!cancellationToken.IsCancellationRequested)
        {
            if (!_timer.IsRunning)
            {
                _timer.Start();    
            }
            
            await _hubContext.Clients.All.UpdateTime(_dateTimeProvider.Now.TimeOfDay, _sequenceService.IsPeakTime(_dateTimeProvider.Now.TimeOfDay) ? "Peak" : "Normal");
            await DoLoop(cancellationToken);
        }
    }

    private async Task DoLoop(CancellationToken cancellationToken)
    {
        _logger.LogDebug($"Transitioning: {_isTransitioning}, Elapsed: '{_elapsed}, CurrentFlowDuration: '{_currentFlow.Duration}'");
        
        if (!_isTransitioning && _elapsed >= _currentFlow.Duration - _defaultWarningDuration)
        {
            _logger.LogDebug($"Switching from {_currentFlow.Name} to {_sequenceService.CurrentSequence.Peek().Name}");
            _isTransitioning = true;
        }
        else if (_elapsed >= _currentFlow.Duration)
        {
            await UpdateFlow();
            _isTransitioning = false;
            _timer.Restart();
            _elapsed = 0;
        }
        var status = TrafficLightStatusProvider.Build(_currentFlow, _sequenceService.CurrentSequence.Peek(), _isTransitioning);
        await UpdateLights(status);
        
        _elapsed = Convert.ToInt32(_timer.ElapsedMilliseconds);
        await Task.Delay(_delay, cancellationToken);
    }

    private async Task UpdateLights(TrafficLightStatusProvider statusProvider)
    {
        foreach (var light in statusProvider.All)
        {
            await _hubContext.Clients.All.UpdateTrafficLight(light.Key, light.State.ToString().ToLower());
        }
    }

    private async Task UpdateFlow()
    {
        _currentFlow = _sequenceService.GetNextFlow(_dateTimeProvider.Now.TimeOfDay);
        await _hubContext.Clients.All.UpdateCurrentFlowInfo(_currentFlow.Name, _sequenceService.CurrentSequence.Peek().Name);
        
        _logger.LogInformation($"Current flow: '{_currentFlow.Name}'");
    }
}

