using TrafficLights.Config;
using TrafficLights.Hubs;
using TrafficLights.Providers;
using TrafficLights.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
builder.Services.AddHostedService<TrafficLightService>();

builder.Services.Configure<TrafficLightSettings>(builder.Configuration.GetSection("TrafficLightSettings"));

builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
builder.Services.AddSingleton<ISequenceService, SequenceService>();

var app = builder.Build();
app.UseRouting();
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<TrafficHub>("/hub");
});

app.Run();
