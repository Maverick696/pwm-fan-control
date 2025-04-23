using FanCommander;
using FanCommander.Services;
using FanCommander.Console;
using FanCommander.Models;
using System.Globalization;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<FanCommanderSettings>(
    builder.Configuration.GetSection("FanCommanderSettings")
);
builder.Services.AddSingleton<IFanService, FanService>();
builder.Services.AddSingleton<ITemperatureService, TemperatureService>();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
//if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
//{
builder.Services.AddSingleton<IConsoleDisplayService, ConsoleDisplayService>();
//}
builder.Services.AddHostedService<Worker>();

// Imposta la cultura in base a variabile d'ambiente (default: en-US)
var lang = Environment.GetEnvironmentVariable("APP_LANG") ?? Environment.GetEnvironmentVariable("LANG") ?? "en";
var culture = lang.StartsWith("it") ? "it-IT" : "en-US";
CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(culture);
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(culture);


var host = builder.Build();
host.Run();
