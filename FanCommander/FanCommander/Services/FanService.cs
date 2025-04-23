using FanCommander.Models;
using FanCommander.Hardware;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Localization;

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
    private readonly IStringLocalizer<FanService> _localizer;

    public FanService(ILogger<FanService> logger, IOptions<FanCommanderSettings> options, IStringLocalizer<FanService> localizer)
    {
        _logger = logger;
        _settings = options.Value;
        _fanController = new GpioFanController(_settings.PwmPin, _settings.PwmFrequency);
        _localizer = localizer;
    }

    public void Start()
    {
        _fanController.Start();
        var msg = _localizer["FanStarted"].Value
            .Replace("{pin}", _settings.PwmPin.ToString())
            .Replace("{freq}", _settings.PwmFrequency.ToString());
        _logger.LogInformation(msg);
    }

    public void SetFanSpeed(int percent)
    {
        double duty = Math.Clamp(percent, 0, 100) / 100.0;
        _fanController.SetDutyCycle(duty);
        var msg = _localizer["FanSetSpeed"].Value.Replace("{duty}", duty.ToString("P0"));
        _logger.LogDebug(msg);
    }

    public void SetMaxSpeed()
    {
        _fanController.SetDutyCycle(_settings.MaxFanSpeed / 100.0);
        _logger.LogInformation(_localizer["FanMaxSpeed"].Value);
    }

    public void Stop()
    {
        _fanController.Stop();
        _logger.LogInformation(_localizer["FanStopped"].Value);
    }

    public void Dispose()
    {
        _fanController?.Dispose();
    }
}
