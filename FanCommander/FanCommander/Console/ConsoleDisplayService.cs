using FanCommander.Models;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Localization;

namespace FanCommander.Console;

public interface IConsoleDisplayService
{
    void Update(double temperature, int fanSpeed);
    void AddLog(string message);
}

public class ConsoleDisplayService : IConsoleDisplayService
{
    private const int GraphWidth = 60;
    private const int GraphHeight = 10;
    private const int LogLines = 8;
    private readonly HistoryBuffer<double> _tempHistory;
    private readonly HistoryBuffer<int> _fanHistory;
    private readonly HistoryBuffer<string> _logBuffer;
    private readonly IStringLocalizer<ConsoleDisplayService> _localizer;

    public ConsoleDisplayService(IStringLocalizer<ConsoleDisplayService> localizer)
    {
        _tempHistory = new HistoryBuffer<double>(GraphWidth);
        _fanHistory = new HistoryBuffer<int>(GraphWidth);
        _logBuffer = new HistoryBuffer<string>(LogLines);
        _localizer = localizer;
    }

    public void Update(double temperature, int fanSpeed)
    {
        _tempHistory.Add(temperature);
        _fanHistory.Add(fanSpeed);
        ClearConsole();
        var title = _localizer["ConsoleTitle"].Value;
        var border = new string('=', title.Length);
        System.Console.WriteLine($"\n{border}\n{title}\n{border}\n");
        System.Console.WriteLine(string.Format(_localizer["ConsoleStatus"], temperature, fanSpeed));
        System.Console.WriteLine(DrawGraph(_tempHistory.GetAll()));
        System.Console.WriteLine(DrawFanGauge(fanSpeed));
        System.Console.WriteLine();
        System.Console.WriteLine(_localizer["ConsoleLog"]);
        foreach (var log in _logBuffer.GetAll())
            System.Console.WriteLine(log);
    }

    public void AddLog(string message)
    {
        _logBuffer.Add(message);
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
            return _localizer["ConsoleGraphCollecting"] + "\n";
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
        result.Add(_localizer["ConsoleGraphTime"]);
        var legend = _localizer["ConsoleGraphLegend"].Value.Replace("{redDot}", "\u001b[31m●\u001b[0m");
        result.Add(legend);
        return string.Join("\n", result);
    }

    private string DrawFanGauge(int fanSpeed)
    {
        int gaugeWidth = GraphWidth;
        int fillWidth = (int)(fanSpeed / 100.0 * gaugeWidth);
        string fill = "\u001b[34m" + new string('█', fillWidth) + new string('░', gaugeWidth - fillWidth) + "\u001b[0m";
        string gaugeLabel = _localizer["ConsoleGaugeLabel"];
        int labelLength = gaugeLabel.Length;
        var result = new List<string>();
        result.Add($"\n{gaugeLabel}{fill} {fanSpeed}%");
        result.Add($"{new string(' ', labelLength)}0%{new string(' ', gaugeWidth - 5)}100%");
        return string.Join("\n", result);
    }
}
