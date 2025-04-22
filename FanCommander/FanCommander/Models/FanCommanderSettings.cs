namespace FanCommander.Models;

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
