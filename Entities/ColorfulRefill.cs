using System;
using System.Reflection;
using Celeste.Mod.DJMapHelper.Extensions;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

// ReSharper disable MemberCanBePrivate.Global
namespace Celeste.Mod.DJMapHelper.Entities; 

[CustomEntity("DJMapHelper/colorfulRefill")]
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
        On.Celeste.Player.UseRefill += PlayerUseRefill;
    }

    public static void OnUnload() {
        On.Celeste.Refill.OnPlayer -= RefillOnPlayer;
        On.Celeste.Player.UseRefill -= PlayerUseRefill;
    }

    private static void RefillOnPlayer(On.Celeste.Refill.orig_OnPlayer orig, Refill self, Player player) {
        if (self.GetType() == typeof(ColorfulRefill)) {
            new DynData<Player>(player)["DJMapHelper_RefillColor"] = ((ColorfulRefill) self).color;
        }

        orig(self, player);
    }

    private static bool PlayerUseRefill(On.Celeste.Player.orig_UseRefill orig, Player self, bool twoDashes) {
        DynData<Player> selfData = new DynData<Player>(self);
        if (selfData.Data.ContainsKey("DJMapHelper_RefillColor") && selfData["DJMapHelper_RefillColor"] != null) {
            bool result;

            switch (selfData.Get<RefillColor>("DJMapHelper_RefillColor")) {
                case RefillColor.Red:
                    result = PlayerUseRedRefill(self);
                    break;
                case RefillColor.Blue:
                    result = PlayerUseBlueRefill(self);
                    break;
                case RefillColor.Black:
                    result = PlayerUseBlackRefill(self);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(color), selfData.Get<RefillColor>("DJMapHelper_RefillColor"), null);
            }

            selfData["DJMapHelper_RefillColor"] = null;
            return result;
        } else {
            return orig(self, twoDashes);
        }
    }

    private static bool PlayerUseRedRefill(Player self) {
        return self.RefillDash();
    }

    private static bool PlayerUseBlueRefill(Player self) {
        if (self.Stamina <= 20.0) {
            self.RefillStamina();
            return true;
        }

        return false;
    }

    private static bool PlayerUseBlackRefill(Player self) {
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