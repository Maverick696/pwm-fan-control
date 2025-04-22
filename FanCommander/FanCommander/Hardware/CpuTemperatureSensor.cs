namespace FanCommander.Hardware;

public class CpuTemperatureSensor
{
    private readonly string _thermalZonePath;
    public CpuTemperatureSensor(string thermalZonePath = "/sys/class/thermal/thermal_zone0/temp")
    {
        _thermalZonePath = thermalZonePath;
    }
    public double ReadTemperature()
    {
        try
        {
            string tempStr = File.ReadAllText(_thermalZonePath);
            return double.Parse(tempStr.Trim()) / 1000.0;
        }
        catch
        {
            return 50.0;
        }
    }
}
