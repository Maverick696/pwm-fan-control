using System.Device.Gpio;
using System.Device.Pwm;
using System.Device.Pwm.Drivers;
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

    public Worker(ILogger<Worker> logger, IOptions<FanCommanderSettings> options, IFanService fanService, ITemperatureService temperatureService)
    {
        _logger = logger;
        _settings = options.Value;
        _fanService = fanService;
        _temperatureService = temperatureService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("FanCommander avviato. PWM su GPIO{pin} a {freq}Hz", _settings.PwmPin, _settings.PwmFrequency);
        _fanService.Start();
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                double temperature = _temperatureService.GetCpuTemperature();
                double clampedTemp = Math.Clamp(temperature, _settings.MinTemp, _settings.MaxTemp);
                int fanSpeed = (int)Renormalizer.Renormalize(clampedTemp, _settings.MinTemp, _settings.MaxTemp, _settings.MinSpeed, _settings.MaxSpeed);
                _fanService.SetFanSpeed(fanSpeed);
                _logger.LogInformation("Temp: {temp:F1}°C | Fan: {fan}%", temperature, fanSpeed);
                await Task.Delay(_settings.UpdateIntervalMs, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Interruzione richiesta. Imposto ventola al massimo.");
        }
        finally
        {
            _fanService.SetMaxSpeed();
            await Task.Delay(1000); // Breve attesa
            _fanService.Stop();
        }
    }
}
