using FanCommander.Hardware;

namespace FanCommander.Services;

public interface ITemperatureService
{
    double GetCpuTemperature();
}

public class TemperatureService : ITemperatureService
{
    private readonly ILogger<TemperatureService> _logger;
    private readonly CpuTemperatureSensor _sensor;
    public TemperatureService(ILogger<TemperatureService> logger)
    {
        _logger = logger;
        _sensor = new CpuTemperatureSensor();
    }

    public double GetCpuTemperature()
    {
        try
        {
            return _sensor.ReadTemperature();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Could not read temperature from thermal zone. Using fallback value."
            );
            return 50.0;
        }
    }
}
