using TrafficLights.Hubs;
using TrafficLights.Traffic;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
builder.Services.AddHostedService<Worker>();

var app = builder.Build();
app.UseRouting();
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<TrafficHub>("/hub");
});

app.Run();
