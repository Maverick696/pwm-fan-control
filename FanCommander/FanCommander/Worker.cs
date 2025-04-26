using FanCommander.Models;
using FanCommander.Services;
using FanCommander.Utils;
using Microsoft.Extensions.Options;

namespace FanCommander;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly FanCommanderSettings _settings;
    private readonly IFanService _fanService;
    private readonly ITemperatureService _temperatureService;

    public Worker(
        ILogger<Worker> logger,
        IOptions<FanCommanderSettings> options,
        IFanService fanService,
        ITemperatureService temperatureService)
    {
        _logger = logger;
        _settings = options.Value;
        _fanService = fanService;
        _temperatureService = temperatureService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"FanCommander worker started. PWM on GPIO{_settings.PwmPin} at {_settings.PwmFrequency}Hz");
        try
        {
            _fanService.Start();
        }
        catch (Exception ex)
        {
            string errMsg = "Hardware initialization error (PWM/GPIO). Check Docker permissions and devices.";
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
                _logger.LogInformation($"Temp: {temperature:F1}°C | Fan: {fanSpeed}%");
                await Task.Delay(_settings.UpdateIntervalMs, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            string stopMsg = "Shutdown requested. Setting fan to max speed.";
            _logger.LogInformation(stopMsg);
        }
        finally
        {
            _fanService.SetMaxSpeed();
            await Task.Delay(1000); // Breve attesa
            _fanService.Stop(); // TODO: Dispose del controller PWM and leave the fan on and at max speed.
        }
    }
}
