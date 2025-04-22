using FanCommander;
using FanCommander.Services;
using FanCommander.Console;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<FanCommanderSettings>(
    builder.Configuration.GetSection("FanCommanderSettings")
);
builder.Services.AddSingleton<IFanService, FanService>();
builder.Services.AddSingleton<ITemperatureService, TemperatureService>();
if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
{
    builder.Services.AddSingleton<IConsoleDisplayService, ConsoleDisplayService>();
}
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
