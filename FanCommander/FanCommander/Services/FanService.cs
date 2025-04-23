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
        _logger.LogInformation(_localizer["FanStarted"], _settings.PwmPin, _settings.PwmFrequency);
    }

    public void SetFanSpeed(int percent)
    {
        double duty = Math.Clamp(percent, 0, 100) / 100.0;
        _fanController.SetDutyCycle(duty);
        _logger.LogDebug(_localizer["FanSetSpeed"], duty);
    }

    public void SetMaxSpeed()
    {
        _fanController.SetDutyCycle(_settings.MaxFanSpeed / 100.0);
        _logger.LogInformation(_localizer["FanMaxSpeed"]);
    }

    public void Stop()
    {
        _fanController.Stop();
        _logger.LogInformation(_localizer["FanStopped"]);
    }

    public void Dispose()
    {
        _fanController?.Dispose();
    }
}
