using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DJMapHelper {
    [Tracked]
    public class FeatherBarrier : Solid {
        private static readonly FieldInfo StarFlyTimerFieldInfo = typeof(Player).GetPrivateField("starFlyTimer");
        private static readonly FieldInfo StarFlyColorFieldInfo = typeof(Player).GetPrivateField("starFlyColor");

        private readonly List<FeatherBarrier> adjacent = new List<FeatherBarrier>();
        private float flash;
        private bool flashing;
        private float offX;
        private float offY;
        private readonly List<Vector2> particles = new List<Vector2>();
        private readonly float[] speeds = {12f, 20f, 40f};
        private readonly MTexture temp;

        private readonly Color barrieColor;

        // ReSharper disable once MemberCanBePrivate.Global
        public FeatherBarrier(Vector2 position, float width, float height, ColorfulFlyFeather.FeatherColor color)
            : base(position, width, height, false) {
            switch (color) {
                case ColorfulFlyFeather.FeatherColor.Blue:
                    barrieColor = ColorfulFlyFeather.BlueStarFlyColor;
                    break;
                case ColorfulFlyFeather.FeatherColor.Green:
                    barrieColor = ColorfulFlyFeather.GreenStarFlyColor;
                    break;
                case ColorfulFlyFeather.FeatherColor.Red:
                    barrieColor = ColorfulFlyFeather.RedStarFlyColor;
                    break;
                case ColorfulFlyFeather.FeatherColor.Yellow:
                    barrieColor = ColorfulFlyFeather.OrigStarFlyColor;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(color), color, null);
            }

            Collidable = false;
            temp = new MTexture();
            for (int index = 0; (double) index < (double) Width * (double) Height / 16.0; ++index)
                particles.Add(new Vector2(Calc.Random.NextFloat(Width - 1f),
                    Calc.Random.NextFloat(Height - 1f)));
            offX = position.X;
            offY = position.Y;
            while (offX < 0.0)
                offX += 128f;
            while (offY < 0.0)
                offY += 128f;
            Add(new DisplacementRenderHook(RenderDisplacement));
        }

        public FeatherBarrier(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height,
                data.Enum("color", ColorfulFlyFeather.FeatherColor.Blue)) { }

        public override void Update() {
            offX += Engine.DeltaTime * 12f;
            offY += Engine.DeltaTime * 12f;
            if (flashing) {
                flash = Calc.Approach(flash, 0.0f, Engine.DeltaTime * 5f);
                if (flash <= 0.0)
                    flashing = false;
            }

            int length = speeds.Length;
            int index = 0;
            for (int count = particles.Count; index < count; ++index) {
                Vector2 vector2 = particles[index] +
                                  Vector2.UnitY * speeds[index % length] * Engine.DeltaTime;
                vector2.Y %= Height - 1f;
                particles[index] = vector2;
            }

            base.Update();
        }

        private void OnReflectFeather() {
            flash = 1f;
            flashing = true;
            Scene.CollideInto(
                new Rectangle((int) X, (int) Y - 2, (int) Width, (int) Height + 4), adjacent);
            Scene.CollideInto(
                new Rectangle((int) X - 2, (int) Y, (int) Width + 4, (int) Height), adjacent);
            foreach (FeatherBarrier featherBarrier in adjacent)
                if (!featherBarrier.flashing)
                    featherBarrier.OnReflectFeather();

            adjacent.Clear();
        }

        private void RenderDisplacement() {
            MTexture mTexture = GFX.Game["util/displacementBlock"];
            Color color = barrieColor * 0.3f;
            for (int index1 = 0; (double) index1 < (double) Width; index1 += 128)
            for (int index2 = 0; (double) index2 < (double) Height; index2 += 128) {
                mTexture.GetSubtexture((int) (offX % 128.0), (int) (offY % 128.0),
                    (int) Math.Min(128f, Width - index1),
                    (int) Math.Min(128f, Height - index2), temp);
                temp.Draw(Position + new Vector2(index1, index2), Vector2.Zero, color);
            }
        }

        public override void Render() {
            Draw.Rect(Collider, barrieColor * 0.2f);
            if (flash > 0.0)
                Draw.Rect(Collider, barrieColor * flash);
            Color color = barrieColor * 0.5f;
            foreach (Vector2 particle in particles)
                Draw.Pixel.Draw(Position + particle, Vector2.Zero, color);
        }

        public static void OnLoad() {
            On.Celeste.Player.Update += PlayerOnUpdate;
            On.Celeste.Player.OnCollideH += PlayerOnOnCollideH;
            On.Celeste.Player.OnCollideV += PlayerOnOnCollideV;
        }

        public static void OnUnload() {
            On.Celeste.Player.Update -= PlayerOnUpdate;
            On.Celeste.Player.OnCollideH -= PlayerOnOnCollideH;
            On.Celeste.Player.OnCollideV -= PlayerOnOnCollideV;
        }

        private static void PlayerOnOnCollideH(On.Celeste.Player.orig_OnCollideH orig, Player self,
            CollisionData data) {
            orig(self, data);
            OnCollide(self, data);
        }

        private static void PlayerOnOnCollideV(On.Celeste.Player.orig_OnCollideV orig, Player self,
            CollisionData data) {
            orig(self, data);
            OnCollide(self, data);
        }

        private new static void OnCollide(Player self, CollisionData data) {
            if (self.StateMachine.State == Player.StStarFly) {
                if ((float) StarFlyTimerFieldInfo.GetValue(self) >= 0.2 &&
                    data.Hit is FeatherBarrier barrier) {
                    barrier.OnReflectFeather();
                }
            }
        }

        private static void PlayerOnUpdate(On.Celeste.Player.orig_Update orig, Player self) {
            List<FeatherBarrier> featherBarriers =
                self.Scene.Tracker.GetEntities<FeatherBarrier>().Cast<FeatherBarrier>().ToList();
            foreach (FeatherBarrier featherBarrier in featherBarriers) {
                if ((Color) StarFlyColorFieldInfo.GetValue(self) != featherBarrier.barrieColor ||
                    self.StateMachine.State != Player.StStarFly) {
                    featherBarrier.Collidable = true;
                }
            }

            if (self.CollideFirst<FeatherBarrier>() is FeatherBarrier barrier && self.StateMachine.State != Player.StStarFly) {
                if (SaveData.Instance.Assists.Invincible) {
                    barrier.Collidable = false;
                }
                else {
                    self.Die(Vector2.UnitX * (int) self.Facing);
                }
            }

            orig(self);

            foreach (FeatherBarrier featherBarrier in featherBarriers) {
                featherBarrier.Collidable = false;
            }
        }
    }
}