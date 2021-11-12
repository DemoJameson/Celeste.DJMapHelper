using System;
using System.Reflection;
using Celeste.Mod.DJMapHelper.Extensions;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DJMapHelper.Entities {
    [Tracked]
    [CustomEntity("DJMapHelper/colorfulFlyFeather")]
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

        private static SpriteBank MySpriteBank;

        private readonly Color starFlyColor;
        private FeatherColor color;

        private Lazy<Color> FlyPowerHairColor => new Lazy<Color>(() => StarFlyColorAddHsv(12, -0.09, -0.06));
        private Lazy<Color> FlyPowerHairColor2 => new Lazy<Color>(() => StarFlyColorAddHsv(12, -0.09));
        private Lazy<Color> RespawnColor => new Lazy<Color>(() => StarFlyColorAddHsv(-8, -0.28));
        private Lazy<Color> RespawnColor2 => new Lazy<Color>(() => StarFlyColorAddHsv(11));

        // ReSharper disable once MemberCanBePrivate.Global
        public ColorfulFlyFeather(Vector2 position, bool shielded, bool singleUse, FeatherColor color) : base(position,
            shielded, singleUse) {
            Sprite sprite = Get<Sprite>();
            this.color = color;

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (color) {
                case FeatherColor.Blue:
                    MySpriteBank.CreateOn(sprite, "blueFlyFeather");
                    starFlyColor = BlueStarFlyColor;
                    break;
                case FeatherColor.Green:
                    MySpriteBank.CreateOn(sprite, "greenFlyFeather");
                    starFlyColor = GreenStarFlyColor;
                    break;
                case FeatherColor.Red:
                    MySpriteBank.CreateOn(sprite, "redFlyFeather");
                    starFlyColor = RedStarFlyColor;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(color), color, null);
            }
        }

        // ReSharper disable once UnusedMember.Global
        public ColorfulFlyFeather(EntityData data, Vector2 offset) : this(
            data.Position + offset, data.Bool("shielded"), data.Bool("singleUse"),
            data.Enum(nameof(color), FeatherColor.Blue)) { }

        private Color StarFlyColorAddHsv(int hue, double saturation = 0, double value = 0) {
            return starFlyColor.AddHsv(hue, saturation, value);
        }

        public static void OnLoad() {
            On.Celeste.FlyFeather.OnPlayer += FlyFeatherOnPlayer;
            On.Celeste.FlyFeather.Respawn += FlyFeatherOnRespawn;
        }

        public static void OnUnload() {
            On.Celeste.FlyFeather.OnPlayer -= FlyFeatherOnPlayer;
            On.Celeste.FlyFeather.Respawn -= FlyFeatherOnRespawn;
        }

        public static void OnLoadContent() {
            MySpriteBank = new SpriteBank(GFX.Game, "Graphics/DJMapHelperSprites.xml");
        }

        private static void FlyFeatherOnPlayer(On.Celeste.FlyFeather.orig_OnPlayer orig, FlyFeather self,
            Player player) {
            Color starFlyColor;
            if (self.GetType() == typeof(ColorfulFlyFeather)) {
                ColorfulFlyFeather colorfulFlyFeather = (ColorfulFlyFeather) self;
                starFlyColor = colorfulFlyFeather.starFlyColor;

                P_Collect.Color = colorfulFlyFeather.FlyPowerHairColor.Value;
                P_Collect.Color2 = colorfulFlyFeather.FlyPowerHairColor2.Value;
                P_Boost.Color = colorfulFlyFeather.FlyPowerHairColor.Value;
                P_Boost.Color2 = colorfulFlyFeather.FlyPowerHairColor2.Value;
                P_Flying.Color = colorfulFlyFeather.FlyPowerHairColor.Value;
                P_Flying.Color2 = colorfulFlyFeather.FlyPowerHairColor2.Value;
            } else {
                starFlyColor = OrigStarFlyColor;

                P_Collect.Color = OrigFlyPowerHairColor;
                P_Collect.Color2 = OrigFlyPowerHairColor2;
                P_Boost.Color = OrigFlyPowerHairColor;
                P_Boost.Color2 = OrigFlyPowerHairColor2;
                P_Flying.Color = OrigFlyPowerHairColor;
                P_Flying.Color2 = OrigFlyPowerHairColor2;
            }

            StarFlyColorFieldInfo?.SetValue(player, starFlyColor);

            orig(self, player);
        }

        private static void FlyFeatherOnRespawn(On.Celeste.FlyFeather.orig_Respawn orig, FlyFeather self) {
            if (self.GetType() == typeof(ColorfulFlyFeather)) {
                ColorfulFlyFeather colorfulFlyFeather = (ColorfulFlyFeather) self;

                P_Respawn.Color = colorfulFlyFeather.RespawnColor.Value;
                P_Respawn.Color2 = colorfulFlyFeather.RespawnColor2.Value;
            } else {
                P_Respawn.Color = OrigRespawnColor;
                P_Respawn.Color2 = OrigRespawnColor2;
            }

            orig(self);
        }
    }
}