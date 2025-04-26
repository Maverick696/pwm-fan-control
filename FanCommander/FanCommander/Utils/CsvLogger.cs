using System.Globalization;

namespace FanCommander.Utils;

public static class CsvLogger
{
    private static readonly object _lock = new();
    private static bool _headerWritten = false;
    private static string? _lastFilePath = null;

    public static void Log(string filePath, DateTime timestamp, double temperature, int fanSpeed)
    {
        lock (_lock)
        {
            bool writeHeader = !_headerWritten || _lastFilePath != filePath || !File.Exists(filePath);
            using var sw = new StreamWriter(filePath, append: true);
            if (writeHeader)
            {
                sw.WriteLine("timestamp,temperature_c,fan_speed_percent");
                _headerWritten = true;
                _lastFilePath = filePath;
            }
            string line = string.Format(CultureInfo.InvariantCulture, "{0:O},{1:F1},{2}", timestamp, temperature, fanSpeed);
            sw.WriteLine(line);
        }
    }

    public static void Clear(string filePath)
    {
        lock (_lock)
        {
            using var sw = new StreamWriter(filePath, append: false);
            sw.WriteLine("timestamp,temperature_c,fan_speed_percent");
            _headerWritten = true;
            _lastFilePath = filePath;
        }
    }
}
