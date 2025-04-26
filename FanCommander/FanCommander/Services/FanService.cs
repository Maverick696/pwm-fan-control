using FanCommander.Models;
using FanCommander.Hardware;
using Microsoft.Extensions.Options;

namespace FanCommander.Services;

public interface IFanService
{
    void SetFanSpeed(int percent);
    void SetMaxSpeed();
    void Start();
    void Stop();
    void Dispose();
}

public class FanService : IFanService, IDisposable
{
    private readonly ILogger<FanService> _logger;
    private readonly FanCommanderSettings _settings;
    private readonly GpioFanController _fanController;

    public FanService(ILogger<FanService> logger, IOptions<FanCommanderSettings> options)
    {
        _logger = logger;
        _settings = options.Value;
        _fanController = new GpioFanController(_settings.PwmPin, _settings.PwmFrequency);
    }

    public void Start()
    {
        _fanController.Start();
        _logger.LogInformation($"FanCommander started. PWM on GPIO{_settings.PwmPin} at {_settings.PwmFrequency}Hz");
    }

    public void SetFanSpeed(int percent)
    {
        double duty = Math.Clamp(percent, 0, 100) / 100.0;
        _fanController.SetDutyCycle(duty);
        _logger.LogDebug($"DutyCycle set to {duty:P0}");
    }

    public void SetMaxSpeed()
    {
        _fanController.SetDutyCycle(_settings.MaxFanSpeed / 100.0);
        _logger.LogInformation("DutyCycle set to max");
    }

    public void Stop()
    {
        _fanController.Stop();
        _logger.LogInformation("PWM stopped");
    }

    public void Dispose()
    {
        _fanController?.Dispose();
        _logger.LogInformation("PWM disposed");
    }
}
