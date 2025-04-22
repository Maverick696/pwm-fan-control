using System.Device.Gpio;
using System.Device.Pwm;
using System.Device.Pwm.Drivers;
using Microsoft.Extensions.Options;

namespace FanCommander;

public class FanCommanderSettings
{
    public int PwmPin { get; set; }
    public int PwmFrequency { get; set; }
    public double MinTemp { get; set; }
    public double MaxTemp { get; set; }
    public int MinSpeed { get; set; }
    public int MaxSpeed { get; set; }
    public int MaxFanSpeed { get; set; }
    public int UpdateIntervalMs { get; set; }
}

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly FanCommanderSettings _settings;

    public Worker(ILogger<Worker> logger, IOptions<FanCommanderSettings> options)
    {
        _logger = logger;
        _settings = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("FanCommander avviato. PWM su GPIO{pin} a {freq}Hz", _settings.PwmPin, _settings.PwmFrequency);
        using var pwm = new SoftwarePwmChannel(_settings.PwmPin, _settings.PwmFrequency, 0, false);
        pwm.Start();
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                double temperature = GetCpuTemperature();
                double clampedTemp = Math.Clamp(temperature, _settings.MinTemp, _settings.MaxTemp);
                int fanSpeed = (int)Renormalize(clampedTemp, _settings.MinTemp, _settings.MaxTemp, _settings.MinSpeed, _settings.MaxSpeed);
                pwm.DutyCycle = fanSpeed / 100.0;
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
            pwm.DutyCycle = _settings.MaxFanSpeed / 100.0;
            await Task.Delay(1000); // Breve attesa
            pwm.Stop();
        }
    }

    private double GetCpuTemperature()
    {
        try
        {
            string tempStr = File.ReadAllText("/sys/class/thermal/thermal_zone0/temp");
            return double.Parse(tempStr.Trim()) / 1000.0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Errore lettura temperatura. Ritorno valore di fallback.");
            return 50.0;
        }
    }

    private double Renormalize(double value, double fromMin, double fromMax, double toMin, double toMax)
    {
        double fromRange = fromMax - fromMin;
        double toRange = toMax - toMin;
        return ((value - fromMin) * toRange / fromRange) + toMin;
    }
}
