using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DJMapHelper {
    [Tracked]
    public class ColorfulFlyFeather : FlyFeather {
        public enum FeatherColor {
            Blue,
            Green,
            Red,
            Yellow
        }

        private static readonly FieldInfo StarFlyColorFieldInfo = typeof(Player).GetPrivateField("starFlyColor");

        public static readonly Color OrigStarFlyColor = Calc.HexToColor("FFD65C");
        private static readonly Color OrigFlyPowerHairColor = Calc.HexToColor("F2EB6D");
        private static readonly Color OrigFlyPowerHairColor2 = Calc.HexToColor("FFF20F");
        private static readonly Color OrigRespawnColor = Calc.HexToColor("FFDCA4");
        private static readonly Color OrigRespawnColor2 = Calc.HexToColor("FFE95E");

        public static readonly Color BlueStarFlyColor = Calc.HexToColor("6DCFF6");
        public static readonly Color GreenStarFlyColor = Calc.HexToColor("66FF66");
        public static readonly Color RedStarFlyColor = Calc.HexToColor("F21E4F");

        private FeatherColor color;
        private readonly Color starFlyColor;
        private Color FlyPowerHairColor => StarFlyColorAddHsv(12, -0.09, -0.06);
        private Color FlyPowerHairColor2 => StarFlyColorAddHsv(12, -0.09);
        private Color RespawnColor => StarFlyColorAddHsv(-8, -0.28);
        private Color RespawnColor2 => StarFlyColorAddHsv(11);

        // ReSharper disable once MemberCanBePrivate.Global
        public ColorfulFlyFeather(Vector2 position, bool shielded, bool singleUse, FeatherColor color) : base(position,
            shielded, singleUse) {
            Sprite sprite;
            this.color = color;

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (color) {
                case FeatherColor.Blue:
                    sprite = DJMapHelperModule.Instance.SpriteBank.Create("blueFlyFeather");
                    starFlyColor = BlueStarFlyColor;
                    break;
                case FeatherColor.Green:
                    sprite = DJMapHelperModule.Instance.SpriteBank.Create("greenFlyFeather");
                    starFlyColor = GreenStarFlyColor;
                    break;
                case FeatherColor.Red:
                    sprite = DJMapHelperModule.Instance.SpriteBank.Create("redFlyFeather");
                    starFlyColor = RedStarFlyColor;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(color), color, null);
            }

            // change sprite;
            typeof(FlyFeather).GetField("sprite", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(this, sprite);
            Remove(Get<Sprite>());
            Add(sprite);
        }

        public ColorfulFlyFeather(EntityData data, Vector2 offset) : this(
            data.Position + offset, data.Bool("shielded"), data.Bool("singleUse"),
            data.Enum(nameof(color), FeatherColor.Blue)) { }


        private Color StarFlyColorAddHsv(int hue, double saturation = 0, double value = 0) {
            ColorToHsv(starFlyColor, out double outHue, out double outSaturation, out double outValue);

            outHue += hue;
            outSaturation += saturation;
            outValue += value;
            
            return ColorFromHsv(outHue, outSaturation, outValue);
        }

        private static void ColorToHsv(Color color, out double hue, out double saturation, out double value) {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            hue = System.Drawing.Color.FromArgb(color.R, color.G, color.B).GetHue();
            saturation = max == 0 ? 0 : 1d - 1d * min / max;
            value = max / 255d;
        }

        private static Color ColorFromHsv(double hue, double saturation, double value) {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

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

        public static void OnLoad() {
            On.Celeste.FlyFeather.OnPlayer += FlyFeatherOnPlayer;
        }

        public static void OnUnload() {
            On.Celeste.FlyFeather.OnPlayer -= FlyFeatherOnPlayer;
        }

        private static void FlyFeatherOnPlayer(On.Celeste.FlyFeather.orig_OnPlayer orig, FlyFeather self,
            Player player) {
            Color starFlyColor;
            if (self.GetType() == typeof(ColorfulFlyFeather)) {
                ColorfulFlyFeather colorfulFlyFeather = (ColorfulFlyFeather) self;
                starFlyColor = colorfulFlyFeather.starFlyColor;

                P_Collect.Color = colorfulFlyFeather.FlyPowerHairColor;
                P_Collect.Color2 = colorfulFlyFeather.FlyPowerHairColor2;
                P_Boost.Color = colorfulFlyFeather.FlyPowerHairColor;
                P_Boost.Color2 = colorfulFlyFeather.FlyPowerHairColor2;
                P_Flying.Color = colorfulFlyFeather.FlyPowerHairColor;
                P_Flying.Color2 = colorfulFlyFeather.FlyPowerHairColor2;
                P_Respawn.Color = colorfulFlyFeather.RespawnColor;
                P_Respawn.Color2 = colorfulFlyFeather.RespawnColor2;
            }
            else {
                starFlyColor = OrigStarFlyColor;

                P_Collect.Color = OrigFlyPowerHairColor;
                P_Collect.Color2 = OrigFlyPowerHairColor2;
                P_Boost.Color = OrigFlyPowerHairColor;
                P_Boost.Color2 = OrigFlyPowerHairColor2;
                P_Flying.Color = OrigFlyPowerHairColor;
                P_Flying.Color2 = OrigFlyPowerHairColor2;
                P_Respawn.Color = OrigRespawnColor;
                P_Respawn.Color2 = OrigRespawnColor2;
            }

            StarFlyColorFieldInfo?.SetValue(player, starFlyColor);

            orig(self, player);
        }
    }
}