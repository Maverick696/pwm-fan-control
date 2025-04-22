using FanCommander.Models;
using System.Runtime.InteropServices;

namespace FanCommander.Console;

public interface IConsoleDisplayService
{
    void Update(double temperature, int fanSpeed);
}

public class ConsoleDisplayService : IConsoleDisplayService
{
    private const int GraphWidth = 60;
    private const int GraphHeight = 10;
    private readonly HistoryBuffer<double> _tempHistory;
    private readonly HistoryBuffer<int> _fanHistory;

    public ConsoleDisplayService()
    {
        _tempHistory = new HistoryBuffer<double>(GraphWidth);
        _fanHistory = new HistoryBuffer<int>(GraphWidth);
    }

    public void Update(double temperature, int fanSpeed)
    {
        _tempHistory.Add(temperature);
        _fanHistory.Add(fanSpeed);
        ClearConsole();
        var title = "PWM FAN Control";
        var border = new string('=', title.Length);
        System.Console.WriteLine($"\n{border}\n{title}\n{border}\n");
        System.Console.WriteLine($"CPU Temperature: {temperature,5:F1}°C    |    Fan Speed: {fanSpeed,3}%\n");
        System.Console.WriteLine(DrawGraph(_tempHistory.GetAll()));
        System.Console.WriteLine(DrawFanGauge(fanSpeed));
    }

    private void ClearConsole()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            System.Console.Clear();
        else
            System.Console.Write("\u001b[H\u001b[J");
    }

    private string DrawGraph(IReadOnlyCollection<double> tempHistory)
    {
        if (tempHistory.Count == 0)
            return "Collecting data for graph...\n";
        var tempList = tempHistory.ToArray();
        double tempMin = tempList.Min();
        double tempMax = tempList.Max();
        double tempRange = tempMax - tempMin == 0 ? 1 : tempMax - tempMin;
        var graph = new string[GraphHeight, GraphWidth];
        for (int y = 0; y < GraphHeight; y++)
            for (int x = 0; x < GraphWidth; x++)
                graph[y, x] = " ";
        for (int i = 0; i < tempList.Length && i < GraphWidth; i++)
        {
            double temp = tempList[i];
            int yPos = GraphHeight - 1 - (int)((temp - tempMin) / tempRange * (GraphHeight - 1));
            yPos = Math.Max(0, Math.Min(GraphHeight - 1, yPos));
            graph[yPos, i] = "\u001b[31m●\u001b[0m"; // RED
        }
        var result = new List<string>();
        for (int y = 0; y < GraphHeight; y++)
        {
            double tempValue = tempMax - (y * tempRange / (GraphHeight - 1));
            string yLabel = (y == 0 || y == GraphHeight - 1 || y == GraphHeight / 2)
                ? $"{tempValue,4:F1}°C |"
                : "       |";
            string row = yLabel + string.Concat(Enumerable.Range(0, GraphWidth).Select(x => graph[y, x]));
            result.Add(row);
        }
        result.Add("       +" + new string('-', GraphWidth));
        result.Add("       Time →");
        result.Add("  Legend: \u001b[31m●\u001b[0m CPU Temperature");
        return string.Join("\n", result);
    }

    private string DrawFanGauge(int fanSpeed)
    {
        int gaugeWidth = GraphWidth;
        int fillWidth = (int)(fanSpeed / 100.0 * gaugeWidth);
        string fill = "\u001b[34m" + new string('█', fillWidth) + new string('░', gaugeWidth - fillWidth) + "\u001b[0m";
        string gaugeLabel = "Fan Speed: ";
        int labelLength = gaugeLabel.Length;
        var result = new List<string>();
        result.Add($"\n{gaugeLabel}{fill} {fanSpeed}%");
        result.Add($"{new string(' ', labelLength)}0%{new string(' ', gaugeWidth - 5)}100%");
        return string.Join("\n", result);
    }
}
