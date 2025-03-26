// Converted from Michael Klements' Python script
// For PWM Fan Control On A Raspberry Pi
// Sets fan speed proportional to CPU temperature - best for good quality fans
// Works with a Pi Desktop Case with OLED Stats Display

using System;
using System.Device.Gpio;
using System.Device.Pwm;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace PwmFanControl.Console
{
    class Program
    {
        // Temperature and speed range variables, edit these to adjust max and min temperatures and speeds
        private const int MinTemp = 25;
        private const int MaxTemp = 80;
        private const int MinSpeed = 0;
        private const int MaxSpeed = 100;

        // GPIO pin for fan control (equivalent to GPIO14 in Python script)
        private const int FanPin = 14;
        // PWM frequency (this should match your fan's specified PWM frequency)
        private const int PwmFrequency = 100;
        
        // Sleep time between temperature checks in milliseconds
        private const int SleepTimeMs = 5000;

        static async Task Main(string[] args)
        {
            System.Console.WriteLine("Starting PWM Fan Control for Raspberry Pi...");
            
            // Create a PWM channel for the fan
            using PwmChannel pwm = PwmChannel.Create(0, FanPin, PwmFrequency, 0);
            
            try
            {
                pwm.Start();
                
                // Main control loop
                while (true)
                {
                    // Get current CPU temperature
                    double temp = GetCpuTemperature();
                    System.Console.WriteLine($"Current CPU temperature: {temp:F1}°C");
                    
                    // Constrain temperature to set range limits
                    if (temp < MinTemp)
                    {
                        temp = MinTemp;
                    }
                    else if (temp > MaxTemp)
                    {
                        temp = MaxTemp;
                    }
                    
                    // Scale the temperature to the fan speed range
                    double dutyCycle = Renormalize(temp, MinTemp, MaxTemp, MinSpeed, MaxSpeed);
                    int dutyCycleInt = (int)Math.Round(dutyCycle);
                    
                    // Set fan duty cycle based on temperature
                    pwm.DutyCycle = dutyCycleInt / 100.0;
                    System.Console.WriteLine($"Setting fan speed to {dutyCycleInt}%");
                    
                    // Sleep for the specified time
                    await Task.Delay(SleepTimeMs);
                }
            }
            finally
            {
                // Ensure the PWM channel is stopped properly
                if (pwm != null)
                {
                    pwm.Stop();
                    pwm.Dispose();
                }
            }
        }
        
        /// <summary>
        /// Function to read in the CPU temperature and return it as a double in degrees Celsius
        /// </summary>
        /// <returns>CPU temperature in degrees Celsius</returns>
        private static double GetCpuTemperature()
        {
            try
            {
                // On Raspberry Pi, we would use 'vcgencmd measure_temp'
                // This is a simulation for other platforms
                if (OperatingSystem.IsLinux())
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "vcgencmd",
                        Arguments = "measure_temp",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using var process = Process.Start(startInfo);
                    if (process != null)
                    {
                        string output = process.StandardOutput.ReadToEnd();
                        process.WaitForExit();
                        
                        // Parse output like "temp=45.8'C"
                        var match = Regex.Match(output, @"temp=(\d+\.\d+)");
                        if (match.Success && double.TryParse(match.Groups[1].Value, out double temperature))
                        {
                            return temperature;
                        }
                    }
                }
                
                // Fallback if we can't get the temperature
                // On non-Raspberry Pi platforms, we could read from /sys/class/thermal/thermal_zone0/temp on Linux
                // or use other system-specific methods
                System.Console.WriteLine("Warning: Using simulated temperature. This might not be accurate.");
                return 40.0; // Return a simulated temperature
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error getting CPU temperature: {ex.Message}");
                return 40.0; // Return a default temperature in case of error
            }
        }
        
        /// <summary>
        /// Function to scale a value from one range to another
        /// </summary>
        /// <returns>Scaled value in the target range</returns>
        private static double Renormalize(double value, double fromMin, double fromMax, double toMin, double toMax)
        {
            double fromRange = fromMax - fromMin;
            double toRange = toMax - toMin;
            double valueScaled = (value - fromMin) / fromRange;
            
            return toMin + (valueScaled * toRange);
        }
    }
}
