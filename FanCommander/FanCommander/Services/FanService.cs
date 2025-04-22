using FanCommander.Models;
using FanCommander.Hardware;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FanCommander.Services;

public interface IFanService
{
    void SetFanSpeed(int percent);
    void SetMaxSpeed();
    void Start();
    void Stop();
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
        _logger.LogInformation("PWM avviato su GPIO{pin} a {freq}Hz", _settings.PwmPin, _settings.PwmFrequency);
    }

    public void SetFanSpeed(int percent)
    {
        double duty = Math.Clamp(percent, 0, 100) / 100.0;
        _fanController.SetDutyCycle(duty);
        _logger.LogDebug("DutyCycle impostato a {duty:P0}", duty);
    }

    public void SetMaxSpeed()
    {
        _fanController.SetDutyCycle(_settings.MaxFanSpeed / 100.0);
        _logger.LogInformation("DutyCycle impostato al massimo");
    }

    public void Stop()
    {
        _fanController.Stop();
        _logger.LogInformation("PWM fermato");
    }

    public void Dispose()
    {
        _fanController?.Dispose();
    }
}
