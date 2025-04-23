using FanCommander.Hardware;
using Microsoft.Extensions.Localization;

namespace FanCommander.Services;

public interface ITemperatureService
{
    double GetCpuTemperature();
}

public class TemperatureService : ITemperatureService
{
    private readonly ILogger<TemperatureService> _logger;
    private readonly CpuTemperatureSensor _sensor;
    private readonly IStringLocalizer<TemperatureService> _localizer;
    public TemperatureService(ILogger<TemperatureService> logger, IStringLocalizer<TemperatureService> localizer)
    {
        _logger = logger;
        _sensor = new CpuTemperatureSensor();
        _localizer = localizer;
    }

    public double GetCpuTemperature()
    {
        try
        {
            return _sensor.ReadTemperature();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, _localizer["TempReadError"].Value);
            return 50.0;
        }
    }
}
