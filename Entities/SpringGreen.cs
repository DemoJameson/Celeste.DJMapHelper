using System.Reflection;
using Celeste.Mod.DJMapHelper.Extensions;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DJMapHelper.Entities; 

[CustomEntity("DJMapHelper/springGreen")]
public class SpringGreen : Spring {
    private static readonly FieldInfo SpringSprite = typeof(Spring).GetPrivateField("sprite");
    private static readonly MethodInfo SpriteCloneInto = typeof(Sprite).GetPrivateMethod("CloneInto");

    public SpringGreen(Vector2 position, Orientations orientation, string spritePath)
        : base(position, orientation, true) {
        if (string.IsNullOrWhiteSpace(spritePath)) {
            spritePath = "objects/DJMapHelper/springGreen/";
        }

        if (!spritePath.EndsWith("/")) {
            spritePath += "/";
        }

        Sprite sprite = new(GFX.Game, spritePath);
        sprite.Add("idle", "", 0.0f, new int[1]);
        sprite.Add("bounce", "", 0.07f, "idle", 0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3, 4, 5);
        sprite.Add("disabled", "white", 0.07f);
        sprite.Play("idle");
        sprite.Origin.X = sprite.Width / 2f;
        sprite.Origin.Y = sprite.Height;
        if (orientation == Orientations.WallLeft) {
            sprite.Rotation = 1.570796f;
        } else if (orientation == Orientations.WallRight) {
            sprite.Rotation = -1.570796f;
        }

        if (SpringSprite.GetValue(this) is Sprite origSprite) {
            SpriteCloneInto.Invoke(sprite, new object[] {origSprite});
        }
    }

    public SpringGreen(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Enum("orientation", Orientations.Floor), data.Attr("sprite")) { }

    public static void OnLoad() {
        On.Celeste.Spring.OnCollide += SpringGreenOnCollide;
    }

    public static void OnUnLoad() {
        On.Celeste.Spring.OnCollide -= SpringGreenOnCollide;
    }

    private static void SpringGreenOnCollide(On.Celeste.Spring.orig_OnCollide orig, Spring self, Player player) {
        if (self.GetType() == typeof(SpringGreen)) {
            if (player.StateMachine.State == 9) {
                return;
            }

            var origDashAttacking = player.DashAttacking;
            var origSpeedX = player.Speed.X;
            var origSpeedY = player.Speed.Y;
            if (origDashAttacking && player.Speed == Vector2.Zero) {
                FieldInfo beforeDashSpeedFieldInfo = typeof(Player).GetPrivateField("beforeDashSpeed");
                Vector2 bDSpeed = (Vector2)beforeDashSpeedFieldInfo?.GetValue(player);
                origSpeedX = bDSpeed.X;
                origSpeedY = bDSpeed.Y;
            }

            Vector2 origDashDir = player.DashDir;
            FieldInfo varJumpSpeedFieldInfo = typeof(Player).GetPrivateField("varJumpSpeed");
            player.Speed.X = 0;
            player.Speed.Y = 0;
            orig(self, player);
            if (self.Orientation == Orientations.Floor) {
                player.Speed.X = origSpeedX;
                if (origSpeedY > 0.0f) {
                    if (origDashDir.Y > 0.0f && origDashAttacking) {
                        player.Speed.Y = -370f / 3;
                        varJumpSpeedFieldInfo?.SetValue(player, -370f / 3);
                    } else {
                        player.Speed.Y = -185f;
                    }
                } else {
                    player.Speed.Y = origSpeedY - 185f;
                    varJumpSpeedFieldInfo?.SetValue(player, player.Speed.Y);
                }
            } else if (self.Orientation == Orientations.WallLeft) {
                player.Speed.Y = origSpeedY - 140f;
                if (origSpeedX > 0.0f) {
                    player.Speed.X = origSpeedX + 240f;
                } else {
                    player.Speed.X = 240f;
                }

                varJumpSpeedFieldInfo?.SetValue(player, player.Speed.Y);
            } else {
                player.Speed.Y = origSpeedY - 140f;
                if (origSpeedX < 0.0f) {
                    player.Speed.X = origSpeedX - 240f;
                } else {
                    player.Speed.X = -240f;
                }

                varJumpSpeedFieldInfo?.SetValue(player, player.Speed.Y);
            }
        } else {
            orig(self, player);
        }
    }
}