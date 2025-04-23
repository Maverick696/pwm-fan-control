using System.Device.Pwm.Drivers;

namespace FanCommander.Hardware;

public class GpioFanController : IDisposable
{
    private readonly SoftwarePwmChannel _pwm;
    private bool _started = false;
    public GpioFanController(int pin, int frequency)
    {
        _pwm = new SoftwarePwmChannel(pin, frequency, 0, false);
    }
    public void Start()
    {
        if (!_started)
        {
            _pwm.Start();
            _started = true;
        }
    }
    public void SetDutyCycle(double duty)
    {
        _pwm.DutyCycle = Math.Clamp(duty, 0.0, 1.0);
    }
    public void Stop()
    {
        if (_started)
        {
            _pwm.Stop();
            _started = false;
        }
    }
    public void Dispose()
    {
        _pwm?.Dispose();
    }
}
