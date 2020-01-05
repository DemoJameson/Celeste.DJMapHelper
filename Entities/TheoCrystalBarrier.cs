using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;

namespace Celeste.Mod.DJMapHelper.Entities {
    [Tracked]
    public class TheoCrystalBarrier : Solid {
        private readonly List<TheoCrystalBarrier> adjacent = new List<TheoCrystalBarrier>();
        private readonly List<Vector2> particles = new List<Vector2>();
        private readonly float[] speeds = {12f, 20f, 40f};
        private readonly MTexture temp;
        private float flash;
        private bool flashing;
        private float offX;
        private float offY;

        private TheoCrystalBarrier(Vector2 position, float width, float height)
            : base(position, width, height, false) {
            Collidable = false;
            temp = new MTexture();
            for (var index = 0; (double) index < (double) Width * (double) Height / 16.0; ++index)
                particles.Add(new Vector2(Calc.Random.NextFloat(Width - 1f), Calc.Random.NextFloat(Height - 1f)));

            offX = position.X;
            offY = position.Y;
            while (offX < 0.0) offX += 128f;

            while (offY < 0.0) offY += 128f;

            Add(new DisplacementRenderHook(RenderDisplacement));
            Add(new HoldableCollider(OnTheoCrystal));
        }

        public TheoCrystalBarrier(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height) { }

        private void OnTheoCrystal(Holdable theoCrystal) {
            OnReflect();
        }

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
                Vector2 vector2 = particles[index] + Vector2.UnitY * speeds[index % length] * Engine.DeltaTime;
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
            foreach (TheoCrystalBarrier seekerBarrier in adjacent)
                if (!seekerBarrier.flashing)
                    seekerBarrier.OnReflect();

            adjacent.Clear();
        }

        private void RenderDisplacement() {
            MTexture mTexture = GFX.Game["util/displacementBlock"];
            Color color = Color.White * 0.3f;
            for (var index1 = 0; (double) index1 < (double) Width; index1 += 128)
            for (var index2 = 0; (double) index2 < (double) Height; index2 += 128) {
                mTexture.GetSubtexture((int) (offX % 128.0), (int) (offY % 128.0),
                    (int) Math.Min(128f, Width - index1), (int) Math.Min(128f, Height - index2), temp);
                temp.Draw(Position + new Vector2(index1, index2), Vector2.Zero, color);
            }
        }

        public override void Render() {
            Color backgroundColor = Calc.HexToColor("65ABFF");
            Draw.Rect(Collider, backgroundColor * 0.2f);
            if (flash > 0.0) Draw.Rect(Collider, backgroundColor * flash);

            Color color = Calc.HexToColor("66FF66") * 0.6f;
            foreach (Vector2 particle in particles) Draw.Pixel.Draw(Position + particle, Vector2.Zero, color);
        }

        public static void OnLoad() {
            On.Celeste.TheoCrystal.Update += TheoCrystalOnUpdate;
            On.Celeste.Player.Update += PlayerOnUpdate;
            On.Celeste.Player.Pickup += PlayerOnPickup;
            // 因为 hook On.Celeste.Player.OnCollideH 在 Linux 中会导致游戏崩溃，所以换成 IL
            IL.Celeste.Player.OnCollideH += AddCollideCheck;
            IL.Celeste.Player.OnCollideV += AddCollideCheck;
        }

        public static void OnUnload() {
            On.Celeste.TheoCrystal.Update -= TheoCrystalOnUpdate;
            On.Celeste.Player.Update -= PlayerOnUpdate;
            On.Celeste.Player.Pickup -= PlayerOnPickup;
            IL.Celeste.Player.OnCollideH -= AddCollideCheck;
            IL.Celeste.Player.OnCollideV -= AddCollideCheck;
        }

        private static void AddCollideCheck(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(instruction => instruction.OpCode == OpCodes.Ret)) {
                var className = cursor.Method.Parameters[0].ParameterType.Name;
                Logger.Log("DJMapHelper/TheoCrystalBarrier",
                    $"Adding code to make theo crystal barrier light at index {cursor.Index} in CIL code for {className}.{cursor.Method.Name}");
                cursor.Emit(OpCodes.Ldarg_1);
                cursor.EmitDelegate<Action<CollisionData>>(CheckCollide);
                cursor.GotoNext();
            }
        }

        private static void CheckCollide(CollisionData data) {
            if (data.Hit is TheoCrystalBarrier barrier) barrier.OnReflect();
        }

        private static void TheoCrystalOnUpdate(On.Celeste.TheoCrystal.orig_Update orig, TheoCrystal self) {
            var theoCrystalBarrier = self.Scene.Tracker.GetEntities<TheoCrystalBarrier>().ToList();
            theoCrystalBarrier.ForEach(entity => entity.Collidable = true);
            orig(self);
            theoCrystalBarrier.ForEach(entity => entity.Collidable = false);
        }

        private static void PlayerOnUpdate(On.Celeste.Player.orig_Update orig, Player self) {
            var theoCrystalBarrier = self.Scene.Tracker.GetEntities<TheoCrystalBarrier>().ToList();
            if (self.Holding?.Entity is TheoCrystal) theoCrystalBarrier.ForEach(entity => entity.Collidable = true);

            CollideCheckOutside(self, Vector2.UnitX);
            CollideCheckOutside(self, -Vector2.UnitX);
            CollideCheckOutside(self, Vector2.UnitY * 3);
            CollideCheckOutside(self, -Vector2.UnitY);

            orig(self);
            theoCrystalBarrier.ForEach(entity => entity.Collidable = false);
        }

        private static void CollideCheckOutside(Player player, Vector2 direction) {
            if (player.CollideFirstOutside<TheoCrystalBarrier>(player.Position + direction) is TheoCrystalBarrier
                barrier) {
                if (direction.Abs().Y > 0) direction = 10 * direction - Vector2.UnitX * (int) player.Facing;

                if (direction.Y > 0) {
                    player.PointBounce(player.Center + direction);
                }
                else {
                    On.Celeste.Player.RefillStamina += DisabledRefillStamina;
                    On.Celeste.Player.RefillDash += DisabledRefillDash;
                    player.PointBounce(player.Center + direction);
                    On.Celeste.Player.RefillStamina -= DisabledRefillStamina;
                    On.Celeste.Player.RefillDash -= DisabledRefillDash;
                }

                Audio.Play("event:/game/general/crystalheart_bounce", player.Center + direction);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                barrier.OnReflect();
            }
        }

        private static void DisabledRefillStamina(On.Celeste.Player.orig_RefillStamina orig, Player self) { }

        private static bool DisabledRefillDash(On.Celeste.Player.orig_RefillDash orig, Player self) {
            return false;
        }

        private static bool PlayerOnPickup(On.Celeste.Player.orig_Pickup orig, Player self, Holdable pickup) {
            var theoCrystalBarrier = self.Scene.Tracker.GetEntities<TheoCrystalBarrier>().ToList();
            theoCrystalBarrier.ForEach(entity => entity.Collidable = true);
            var collide = self.CollideCheck<TheoCrystalBarrier>();
            theoCrystalBarrier.ForEach(entity => entity.Collidable = false);

            if (collide && pickup.Entity is TheoCrystal) return false;

            return orig(self, pickup);
        }
    }
}