using System;
using System.Reflection;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.DJMapHelper.Extensions; 

public static class ColorExtensions {
    public static HsvInfo ToHsv(this Color color) {
        int max = Math.Max(color.R, Math.Max(color.G, color.B));
        int min = Math.Min(color.R, Math.Min(color.G, color.B));

        double hue = System.Drawing.Color.FromArgb(color.R, color.G, color.B).GetHue();
        double saturation = max == 0 ? 0 : 1d - 1d * min / max;
        double value = max / 255d;
        return new HsvInfo(hue, saturation, value);
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