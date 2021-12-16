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
        private static readonly Func<FlyFeather, bool> ShieldedGetter = "shielded".CreateDelegate_Get<FlyFeather, bool>();

        public static Color OrigStarFlyColor = Calc.HexToColor("FFD65C");
        private static Color origFlyPowerHairColor = Calc.HexToColor("F2EB6D");
        private static Color origFlyPowerHairColor2 = Calc.HexToColor("FFF20F");
        private static Color origRespawnColor = Calc.HexToColor("FFDCA4");
        private static Color origRespawnColor2 = Calc.HexToColor("FFE95E");

        public static readonly Color BlueStarFlyColor = Calc.HexToColor("6DCFF6");
        public static readonly Color GreenStarFlyColor = Calc.HexToColor("66FF66");
        public static readonly Color RedStarFlyColor = Calc.HexToColor("F21E4F");

        private static SpriteBank mySpriteBank;

        private readonly Color starFlyColor;
        private FeatherColor color;

        private Lazy<Color> FlyPowerHairColor => new(() => StarFlyColorAddHsv(12, -0.09, -0.06));
        private Lazy<Color> FlyPowerHairColor2 => new(() => StarFlyColorAddHsv(12, -0.09));
        private Lazy<Color> RespawnColor => new(() => StarFlyColorAddHsv(-8, -0.28));
        private Lazy<Color> RespawnColor2 => new(() => StarFlyColorAddHsv(11));

        // ReSharper disable once MemberCanBePrivate.Global
        public ColorfulFlyFeather(Vector2 position, bool shielded, bool singleUse, FeatherColor color) : base(position,
            shielded, singleUse) {
            Sprite sprite = Get<Sprite>();
            this.color = color;

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (color) {
                case FeatherColor.Blue:
                    mySpriteBank.CreateOn(sprite, "blueFlyFeather");
                    starFlyColor = BlueStarFlyColor;
                    break;
                case FeatherColor.Green:
                    mySpriteBank.CreateOn(sprite, "greenFlyFeather");
                    starFlyColor = GreenStarFlyColor;
                    break;
                case FeatherColor.Red:
                    mySpriteBank.CreateOn(sprite, "redFlyFeather");
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
            On.Celeste.Player.ctor += PlayerOnCtor;
            On.Celeste.FlyFeather.OnPlayer += FlyFeatherOnPlayer;
            On.Celeste.FlyFeather.Respawn += FlyFeatherOnRespawn;
        }

        public static void OnUnload() {
            On.Celeste.Player.ctor -= PlayerOnCtor;
            On.Celeste.FlyFeather.OnPlayer -= FlyFeatherOnPlayer;
            On.Celeste.FlyFeather.Respawn -= FlyFeatherOnRespawn;
        }

        public static void OnLoadContent() {
            mySpriteBank = new SpriteBank(GFX.Game, "Graphics/DJMapHelperSprites.xml");
            InitOrigColors();
        }

        private static void InitOrigColors() {
            origFlyPowerHairColor = P_Collect?.Color ?? Calc.HexToColor("F2EB6D");
            origFlyPowerHairColor2 = P_Collect?.Color2 ?? Calc.HexToColor("FFF20F");
            origRespawnColor = P_Respawn?.Color ?? Calc.HexToColor("FFDCA4");
            origRespawnColor2 = P_Respawn?.Color2 ?? Calc.HexToColor("FFE95E");
        }

        private static void PlayerOnCtor(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode) {
            orig(self, position, spriteMode);
            OrigStarFlyColor = (Color)StarFlyColorFieldInfo.GetValue(self);
        }

        private static void FlyFeatherOnPlayer(On.Celeste.FlyFeather.orig_OnPlayer orig, FlyFeather self, Player player) {
            if (!ShieldedGetter(self) || player.DashAttacking) {
                Color starFlyColor;
                if (self.GetType() == typeof(ColorfulFlyFeather)) {
                    ColorfulFlyFeather colorfulFlyFeather = (ColorfulFlyFeather)self;
                    starFlyColor = colorfulFlyFeather.starFlyColor;

                    P_Collect.Color = colorfulFlyFeather.FlyPowerHairColor.Value;
                    P_Collect.Color2 = colorfulFlyFeather.FlyPowerHairColor2.Value;
                    P_Boost.Color = colorfulFlyFeather.FlyPowerHairColor.Value;
                    P_Boost.Color2 = colorfulFlyFeather.FlyPowerHairColor2.Value;
                    P_Flying.Color = colorfulFlyFeather.FlyPowerHairColor.Value;
                    P_Flying.Color2 = colorfulFlyFeather.FlyPowerHairColor2.Value;
                } else {
                    starFlyColor = OrigStarFlyColor;

                    P_Collect.Color = origFlyPowerHairColor;
                    P_Collect.Color2 = origFlyPowerHairColor2;
                    P_Boost.Color = origFlyPowerHairColor;
                    P_Boost.Color2 = origFlyPowerHairColor2;
                    P_Flying.Color = origFlyPowerHairColor;
                    P_Flying.Color2 = origFlyPowerHairColor2;
                }

                StarFlyColorFieldInfo.SetValue(player, starFlyColor);
            }

            orig(self, player);
        }

        private static void FlyFeatherOnRespawn(On.Celeste.FlyFeather.orig_Respawn orig, FlyFeather self) {
            if (self.GetType() == typeof(ColorfulFlyFeather)) {
                ColorfulFlyFeather colorfulFlyFeather = (ColorfulFlyFeather)self;

                P_Respawn.Color = colorfulFlyFeather.RespawnColor.Value;
                P_Respawn.Color2 = colorfulFlyFeather.RespawnColor2.Value;
            } else {
                P_Respawn.Color = origRespawnColor;
                P_Respawn.Color2 = origRespawnColor2;
            }

            orig(self);
        }
    }
}