using System;
using System.Reflection;
using Celeste.Mod.DJMapHelper.Extensions;
using Microsoft.Xna.Framework;
using Monocle;

// ReSharper disable MemberCanBePrivate.Global
namespace Celeste.Mod.DJMapHelper.Entities {
    public class ColorfulRefill : Refill {
        public enum RefillColor {
            Red,
            Blue,
            Black
        }

        private static readonly FieldInfo SpriteFieldInfo = typeof(Refill).GetPrivateField("sprite");
        private static readonly FieldInfo FlashFieldInfo = typeof(Refill).GetPrivateField("flash");

        private readonly RefillColor color;

        public ColorfulRefill(Vector2 position, bool twoDashes, bool oneUse, RefillColor color) : base(position,
            twoDashes, oneUse) {
            this.color = color;
            var str = "objects/DJMapHelper/";
            switch (color) {
                case RefillColor.Red:
                    str += "redRefill/";
                    break;
                case RefillColor.Blue:
                    str += "blueRefill/";
                    break;
                case RefillColor.Black:
                    str += "blackRefill/";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(color), color, null);
            }

            Sprite sprite = new Sprite(GFX.Game, str + "idle");
            sprite.AddLoop("idle", "", 0.1f);
            sprite.Play("idle");
            sprite.CenterOrigin();
            Sprite flash = new Sprite(GFX.Game, str + "flash");
            flash.Add(nameof(flash), "", 0.05f);
            flash.OnFinish = anim => flash.Visible = false;
            flash.CenterOrigin();
            SpriteFieldInfo?.SetValue(this, sprite);
            FlashFieldInfo?.SetValue(this, flash);
            Remove(Get<Sprite>());
            Remove(Get<Sprite>());
            Add(sprite);
            Add(flash);
        }

        public ColorfulRefill(EntityData data, Vector2 offset)
            : this(data.Position + offset, false,
                data.Bool("oneUse"),
                data.Enum(nameof(color), RefillColor.Red)) { }

        public static void OnLoad() {
            On.Celeste.Refill.OnPlayer += RefillOnPlayer;
        }

        public static void OnUnload() {
            On.Celeste.Refill.OnPlayer -= RefillOnPlayer;
        }

        private static void RefillOnPlayer(On.Celeste.Refill.orig_OnPlayer orig, Refill self, Player player) {
            if (self.GetType() == typeof(ColorfulRefill)) {
                RefillColor color = ((ColorfulRefill) self).color;
                switch (color) {
                    case RefillColor.Red:
                        On.Celeste.Player.UseRefill += PlayerUseRedRefill;
                        orig(self, player);
                        On.Celeste.Player.UseRefill -= PlayerUseRedRefill;
                        return;
                    case RefillColor.Blue:
                        On.Celeste.Player.UseRefill += PlayerUseBlueRefill;
                        orig(self, player);
                        On.Celeste.Player.UseRefill -= PlayerUseBlueRefill;
                        return;
                    case RefillColor.Black:
                        On.Celeste.Player.UseRefill += PlayerUseBlackRefill;
                        orig(self, player);
                        On.Celeste.Player.UseRefill -= PlayerUseBlackRefill;
                        return;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(color), color, null);
                }
            }

            orig(self, player);
        }

        private static bool PlayerUseRedRefill(On.Celeste.Player.orig_UseRefill orig, Player self, bool twoDashes) {
            var num = self.MaxDashes;
            if (self.Dashes < num) {
                self.Dashes = num;
                return true;
            }

            return false;
        }

        private static bool PlayerUseBlueRefill(On.Celeste.Player.orig_UseRefill orig, Player self, bool twoDashes) {
            if (self.Stamina <= 20.0) {
                self.Stamina = 110f;
                return true;
            }

            return false;
        }

        private static bool PlayerUseBlackRefill(On.Celeste.Player.orig_UseRefill orig, Player self, bool twoDashes) {
            var num = self.MaxDashes;
            if (self.Dashes == 0 && self.Stamina <= 20.0) {
                if (!SaveData.Instance.Assists.Invincible) {
                    self.Die(new Vector2(0.0f, -1f));
                }

                return true;
            }

            if (self.Stamina <= 20.0) {
                self.Dashes = 0;
                self.Stamina = 110f;
                return true;
            }

            if (self.Dashes < num) {
                self.Dashes = num;
                self.Stamina = 0f;
                return true;
            }

            return false;
        }
    }
}