using System;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.DJMapHelper.Extensions;

public static class ColorExtensions {
    public static HsvInfo ToHsv(this Color color) {
        int max = Math.Max(color.R, Math.Max(color.G, color.B));
        int min = Math.Min(color.R, Math.Min(color.G, color.B));

        double hue = GetHue(color);
        double saturation = max == 0 ? 0 : 1d - 1d * min / max;
        double value = max / 255d;
        return new HsvInfo(hue, saturation, value);
    }

    private static double GetHue(Color color) {
        if (color.R == color.G && color.G == color.B)
            return 0.0f;
        float num1 = color.R / (float) byte.MaxValue;
        float num2 = color.G / (float) byte.MaxValue;
        float num3 = color.B / (float) byte.MaxValue;
        float num4 = 0.0f;
        float num5 = num1;
        float num6 = num1;
        if (num2 > (double) num5)
            num5 = num2;
        if (num3 > (double) num5)
            num5 = num3;
        if (num2 < (double) num6)
            num6 = num2;
        if (num3 < (double) num6)
            num6 = num3;
        float num7 = num5 - num6;
        if (num1 == (double) num5)
            num4 = (num2 - num3) / num7;
        else if (num2 == (double) num5)
            num4 = (float) (2.0 + (num3 - (double) num1) / num7);
        else if (num3 == (double) num5)
            num4 = (float) (4.0 + (num1 - (double) num2) / num7);
        float hue = num4 * 60f;
        if (hue < 0.0)
            hue += 360f;
        return hue;
    }

    public static Color ToColor(this HsvInfo hsvInfo) {
        int hi = Convert.ToInt32(Math.Floor(hsvInfo.Hue / 60)) % 6;
        double f = hsvInfo.Hue / 60 - Math.Floor(hsvInfo.Hue / 60);

        hsvInfo.Value *= 255;
        int v = Convert.ToInt32(hsvInfo.Value);
        int p = Convert.ToInt32(hsvInfo.Value * (1 - hsvInfo.Saturation));
        int q = Convert.ToInt32(hsvInfo.Value * (1 - f * hsvInfo.Saturation));
        int t = Convert.ToInt32(hsvInfo.Value * (1 - (1 - f) * hsvInfo.Saturation));

        switch (hi) {
            case 0:
                return new Color(v, t, p);
            case 1:
                return new Color(q, v, p);
            case 2:
                return new Color(p, v, t);
            case 3:
                return new Color(p, q, v);
            case 4:
                return new Color(t, p, v);
            default:
                return new Color(v, p, q);
        }
    }

    public static Color AddHsv(this Color color, int hue, double saturation = 0, double value = 0) {
        HsvInfo hsvInfo = color.ToHsv();

        hsvInfo.Hue += hue;
        hsvInfo.Saturation += saturation;
        hsvInfo.Value += value;

        return hsvInfo.ToColor();
    }
}

public class HsvInfo {
    public double Hue;
    public double Saturation;
    public double Value;
    public HsvInfo(double hue, double saturation, double value) {
        Hue = hue;
        Saturation = saturation;
        Value = value;
    }
}