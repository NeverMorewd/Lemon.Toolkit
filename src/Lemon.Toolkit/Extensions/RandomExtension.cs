using System;

namespace Lemon.Toolkit.Extensions;

public static class RandomExtension
{
    public static double NextDouble(this Random ran, double minValue, double maxValue)
    {
        return ran.NextDouble() * (maxValue - minValue) + minValue;
    }

    public static double NextDouble(this Random ran, double minValue, double maxValue, int decimalPlace)
    {
        double randNum = ran.NextDouble() * (maxValue - minValue) + minValue;
        return Convert.ToDouble(randNum.ToString("f" + decimalPlace));
    }
}