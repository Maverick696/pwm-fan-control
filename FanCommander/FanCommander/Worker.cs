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
        // Calcola il tempo fino alla prossima mezzanotte
        DateTime now = DateTime.Now;
        DateTime nextMidnight = now.Date.AddDays(1);
        TimeSpan delayToMidnight = nextMidnight - now;
        var csvPath = string.IsNullOrWhiteSpace(_settings.CsvLogPath) ? "fancommander_log.csv" : _settings.CsvLogPath;

        // Task per la pulizia giornaliera del log
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(delayToMidnight, stoppingToken);
                while (!stoppingToken.IsCancellationRequested)
                {
                    CsvLogger.Clear(csvPath);
                    await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                }
            }
            catch (TaskCanceledException) { }
        }, stoppingToken);

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
                // Log su file CSV
                CsvLogger.Log(csvPath, DateTime.UtcNow, temperature, fanSpeed);
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
            _fanService.Stop();
            await Task.Delay(1000); // Breve attesa per garantire che il segnale PWM venga applicato
            _fanService.Dispose();
            _logger.LogInformation("FanCommander worker stopped.");
        }
    }
}
