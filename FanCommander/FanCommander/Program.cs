using FanCommander;
using FanCommander.Services;
using FanCommander.Models;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<FanCommanderSettings>(
    builder.Configuration.GetSection("FanCommanderSettings")
);
builder.Services.AddSingleton<IFanService, FanService>();
builder.Services.AddSingleton<ITemperatureService, TemperatureService>();
builder.Services.AddHostedService<Worker>();


var host = builder.Build();
host.Run();
