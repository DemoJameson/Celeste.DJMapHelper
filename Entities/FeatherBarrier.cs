using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Celeste.Mod.DJMapHelper.Extensions;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;

namespace Celeste.Mod.DJMapHelper.Entities {
    [Tracked]
    public class FeatherBarrier : Solid {
        private static readonly FieldInfo StarFlyTimerFieldInfo = typeof(Player).GetPrivateField("starFlyTimer");
        private static readonly FieldInfo StarFlyColorFieldInfo = typeof(Player).GetPrivateField("starFlyColor");

        private readonly List<FeatherBarrier> adjacent = new List<FeatherBarrier>();

        private readonly Color barrieColor;
        private readonly List<Vector2> particles = new List<Vector2>();
        private readonly float[] speeds = {12f, 20f, 40f};
        private readonly MTexture temp;
        private float flash;
        private bool flashing;
        private float offX;
        private float offY;

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
            for (var index = 0; (double) index < (double) Width * (double) Height / 16.0; ++index)
                particles.Add(new Vector2(Calc.Random.NextFloat(Width - 1f),
                    Calc.Random.NextFloat(Height - 1f)));

            offX = position.X;
            offY = position.Y;
            while (offX < 0.0) offX += 128f;

            while (offY < 0.0) offY += 128f;

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
                if (flash <= 0.0) flashing = false;
            }

            var length = speeds.Length;
            var index = 0;
            for (var count = particles.Count; index < count; ++index) {
                Vector2 vector2 = particles[index] +
                                  Vector2.UnitY * speeds[index % length] * Engine.DeltaTime;
                vector2.Y %= Height - 1f;
                particles[index] = vector2;
            }

            base.Update();
        }

        private void OnReflect() {
            flash = 1f;
            flashing = true;
            Scene.CollideInto(new Rectangle((int) X, (int) Y - 2, (int) Width, (int) Height + 4), adjacent);
            Scene.CollideInto(new Rectangle((int) X - 2, (int) Y, (int) Width + 4, (int) Height), adjacent);
            foreach (FeatherBarrier featherBarrier in adjacent)
                if (!featherBarrier.flashing)
                    featherBarrier.OnReflect();

            adjacent.Clear();
        }

        private void RenderDisplacement() {
            MTexture mTexture = GFX.Game["util/displacementBlock"];
            Color color = barrieColor * 0.3f;
            for (var index1 = 0; (double) index1 < (double) Width; index1 += 128)
            for (var index2 = 0; (double) index2 < (double) Height; index2 += 128) {
                mTexture.GetSubtexture((int) (offX % 128.0), (int) (offY % 128.0),
                    (int) Math.Min(128f, Width - index1),
                    (int) Math.Min(128f, Height - index2), temp);
                temp.Draw(Position + new Vector2(index1, index2), Vector2.Zero, color);
            }
        }

        public override void Render() {
            Draw.Rect(Collider, barrieColor * 0.2f);
            if (flash > 0.0) Draw.Rect(Collider, barrieColor * flash);

            Color color = barrieColor * 0.5f;
            foreach (Vector2 particle in particles) Draw.Pixel.Draw(Position + particle, Vector2.Zero, color);
        }

        public static void OnLoad() {
            On.Celeste.Player.Update += PlayerOnUpdate;
            // 因为 hook On.Celeste.Player.OnCollideH 在 Linux 中会导致游戏崩溃，所以换成 IL
            IL.Celeste.Player.OnCollideH += AddCollideCheck;
            IL.Celeste.Player.OnCollideV += AddCollideCheck;
        }

        public static void OnUnload() {
            On.Celeste.Player.Update -= PlayerOnUpdate;
            IL.Celeste.Player.OnCollideH -= AddCollideCheck;
            IL.Celeste.Player.OnCollideV -= AddCollideCheck;
        }

        private static void AddCollideCheck(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(instruction => instruction.OpCode == OpCodes.Ret)) {
                var className = cursor.Method.Parameters[0].ParameterType.Name;
                Logger.Log("DJMapHelper/FeatherBarrier",
                    $"Adding code to make feather barrier light at index {cursor.Index} in CIL code for {className}.{cursor.Method.Name}");
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldarg_1);
                cursor.EmitDelegate<Action<Player, CollisionData>>(CheckCollide);
                cursor.GotoNext();
            }
        }

        private static void CheckCollide(Player player, CollisionData data) {
            if (player.StateMachine.State != Player.StStarFly) return;

            if ((float) StarFlyTimerFieldInfo.GetValue(player) >= 0.2 &&
                data.Hit is FeatherBarrier barrier)
                barrier.OnReflect();
        }

        private static void PlayerOnUpdate(On.Celeste.Player.orig_Update orig, Player self) {
            var featherBarriers =
                self.Scene.Tracker.GetEntities<FeatherBarrier>().Cast<FeatherBarrier>().ToList();
            foreach (FeatherBarrier featherBarrier in featherBarriers)
                if ((Color) StarFlyColorFieldInfo.GetValue(self) != featherBarrier.barrieColor ||
                    self.StateMachine.State != Player.StStarFly)
                    featherBarrier.Collidable = true;

            if (self.CollideFirst<FeatherBarrier>() is FeatherBarrier barrier &&
                self.StateMachine.State != Player.StStarFly) {
                if (SaveData.Instance.Assists.Invincible)
                    barrier.Collidable = false;
                else
                    self.Die(Vector2.UnitX * (int) self.Facing);
            }

            orig(self);

            foreach (FeatherBarrier featherBarrier in featherBarriers) featherBarrier.Collidable = false;
        }
    }
}