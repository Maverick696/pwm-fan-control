using FanCommander.Models;
using FanCommander.Services;
using FanCommander.Utils;
using Microsoft.Extensions.Options;
using FanCommander.Console;
using Microsoft.Extensions.Localization;

namespace FanCommander;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly FanCommanderSettings _settings;
    private readonly IFanService _fanService;
    private readonly ITemperatureService _temperatureService;
    private readonly bool _isDevelopment;
    private readonly IConsoleDisplayService? _consoleDisplayService;
    private readonly IStringLocalizer<Worker> _localizer;

    public Worker(
        ILogger<Worker> logger,
        IOptions<FanCommanderSettings> options,
        IFanService fanService,
        ITemperatureService temperatureService,
        IConsoleDisplayService? consoleDisplayService,
        IStringLocalizer<Worker> localizer)
    {
        _logger = logger;
        _settings = options.Value;
        _fanService = fanService;
        _temperatureService = temperatureService;
        _isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
        _consoleDisplayService = _isDevelopment ? consoleDisplayService : null;
        _localizer = localizer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var startMsg = _localizer["WorkerStarted"].Value
            .Replace("{pin}", _settings.PwmPin.ToString())
            .Replace("{freq}", _settings.PwmFrequency.ToString());
        if (_isDevelopment && _consoleDisplayService != null)
            _consoleDisplayService.AddLog(startMsg);
        _logger.LogInformation(startMsg);
        try
        {
            _fanService.Start();
        }
        catch (Exception ex)
        {
            string errMsg = _localizer["WorkerHardwareError"].Value;
            if (_isDevelopment && _consoleDisplayService != null)
                _consoleDisplayService.AddLog(errMsg + $"\n{ex.Message}");
            _logger.LogError(ex, errMsg);
            return; // termina il worker
        }
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                double temperature = _temperatureService.GetCpuTemperature();
                double clampedTemp = Math.Clamp(temperature, _settings.MinTemp, _settings.MaxTemp);
                int fanSpeed = (int)Renormalizer.Renormalize(clampedTemp, _settings.MinTemp, _settings.MaxTemp, _settings.MinSpeed, _settings.MaxSpeed);
                _fanService.SetFanSpeed(fanSpeed);
                var infoMsg = _localizer["WorkerStatus"].Value
                    .Replace("{temp}", temperature.ToString("F1"))
                    .Replace("{fan}", fanSpeed.ToString());
                if (_isDevelopment && _consoleDisplayService != null)
                {
                    _consoleDisplayService.Update(temperature, fanSpeed);
                    _consoleDisplayService.AddLog(infoMsg);
                }
                _logger.LogInformation(infoMsg);
                await Task.Delay(_settings.UpdateIntervalMs, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            string stopMsg = _localizer["WorkerStopped"].Value;
            if (_isDevelopment && _consoleDisplayService != null)
                _consoleDisplayService.AddLog(stopMsg);
            _logger.LogInformation(stopMsg);
        }
        finally
        {
            _fanService.SetMaxSpeed();
            await Task.Delay(1000); // Breve attesa
            _fanService.Stop();
        }
    }
}
