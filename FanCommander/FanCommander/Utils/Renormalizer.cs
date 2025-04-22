namespace FanCommander.Utils;

public static class Renormalizer
{
    public static double Renormalize(double value, double fromMin, double fromMax, double toMin, double toMax)
    {
        double fromRange = fromMax - fromMin;
        double toRange = toMax - toMin;
        return ((value - fromMin) * toRange / fromRange) + toMin;
    }
}
