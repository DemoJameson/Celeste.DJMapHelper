using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Celeste.Mod.DJMapHelper.Extensions;
using Microsoft.Xna.Framework;
using Monocle;

// ReSharper disable PossibleInvalidCastExceptionInForeachLoop

namespace Celeste.Mod.DJMapHelper.Entities {
    [Tracked]
    public class BadelineProtector : Entity {
        private readonly int maxQuantity;
        private readonly float radius;
        private readonly float respwanTime;
        private readonly float rotationTime;
        private readonly bool clockwise;

        private static readonly MethodInfo SeekerGotBounced = typeof(Seeker).GetPrivateMethod("GotBouncedOn");
        private readonly List<BadelineDummy> badelines;
        private Player player;
        private float respawnTimer;
        private float rotationPercent;

        public BadelineProtector(EntityData data, Vector2 offset) : this(
            data.Int("maxQuantity", 1), data.Int("radius", 24), data.Float("respwanTime", 8f),
            data.Float("rotationTime", 1.8f), data.Bool("clockwise", true)) { }

        private BadelineProtector(int maxQuantity, int radius, float respwanTime, float rotationTime, bool clockwise) {
            this.maxQuantity = maxQuantity;
            this.radius = radius;
            this.respwanTime = respwanTime;
            this.rotationTime = rotationTime;
            this.clockwise = clockwise;

            respawnTimer = 0.0f;
            rotationPercent = 0.0f;
            badelines = new List<BadelineDummy>();
        }

        private static float GetAngle(float rotationPercent) {
            return MathHelper.Lerp(3.141593f, -3.141593f, Easer(rotationPercent));
        }

        private static float Easer(float value) {
            return value;
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if (player == null) {
                RemoveSelf();
            }

            for (int i = 0; i < maxQuantity; i++) {
                AddBadeline(i != 0);
            }
        }

        private void ResetPosition() {
            if (clockwise) {
                rotationPercent -= Engine.DeltaTime / rotationTime;
                ++rotationPercent;
            }
            else {
                rotationPercent += Engine.DeltaTime / rotationTime;
            }

            rotationPercent %= 1f;

            for (var i = 0; i < badelines.Count; i++) {
                BadelineDummy badeline = badelines[i];
                float badelineRotationPercent = (rotationPercent + i * 1.0f / badelines.Count) % 1f;
                badeline.Position = player.Center + Calc.AngleToVector(GetAngle(badelineRotationPercent), radius);
                badeline.Sprite.Scale.X = Math.Sign(badeline.X - player.X);
            }
        }

        public override void Update() {
            base.Update();

            if (player.Dead) {
                RemoveSelf();
            }

            ResetPosition();

            if (respawnTimer > 0.0 && badelines.Count < maxQuantity) {
                respawnTimer -= Engine.DeltaTime;
                if (respawnTimer <= 0.0) {
                    respawnTimer = respwanTime;
                    AddBadeline();
                }
            }

            for (var i = badelines.Count - 1; i >= 0; i--) {
                BadelineDummy badeline = badelines[i];

                foreach (TouchSwitch touchSwitch in Scene.Tracker.GetEntities<TouchSwitch>()) {
                    if (badeline.CollideCheck(touchSwitch) && !touchSwitch.Switch.Activated) {
                        touchSwitch.TurnOn();
                        RemoveBadeline(badeline);
                        break;
                    }
                }

                foreach (Seeker seeker in Scene.Tracker.GetEntities<Seeker>()) {
                    if (badeline.CollideCheck(seeker)) {
                        SeekerGotBounced.Invoke(seeker, new object[] {this});
                        RemoveBadeline(badeline);
                        break;
                    }
                }

                foreach (SeekerBossShot seekerBossShot in Scene.Tracker.GetEntities<SeekerBossShot>()) {
                    if (badeline.CollideCheck(seekerBossShot)) {
                        seekerBossShot.RemoveSelf();
                        RemoveBadeline(badeline);
                        break;
                    }
                }

                foreach (FinalBossShot finalBossShot in Scene.Tracker.GetEntities<FinalBossShot>()) {
                    if (badeline.CollideCheck(finalBossShot)) {
                        finalBossShot.RemoveSelf();
                        RemoveBadeline(badeline);
                        break;
                    }
                }

                foreach (SeekerBoss seekerBoss in Scene.Tracker.GetEntities<SeekerBoss>()) {
                    if (badeline.CollideCheck(seekerBoss)) {
                        RemoveBadeline(badeline);
                        break;
                    }
                }
            }
        }

        private void AddBadeline(bool silent = false) {
            BadelineDummy badeline = new BadelineDummy(Vector2.Zero) {
                Collider = new Hitbox(8f, 9f, -4f, -11f)
            };
            badeline.Add(new Coroutine(Appear(badeline, silent)));
            badelines.Add(badeline);
            ResetPosition();
            Scene.Add(badeline);
        }

        private IEnumerator Appear(BadelineDummy badeline, bool silent = false) {
            if (!silent) {
                Audio.Play("event:/char/badeline/appear", badeline.Position);
            }

            SceneAs<Level>().Displacement.AddBurst(badeline.Center, 0.5f, 24f, 96f, 0.4f);
            SceneAs<Level>().Particles.Emit(BadelineOldsite.P_Vanish, 12, badeline.Center, Vector2.One * 6f);
            yield break;
        }

        private void Disappear(BadelineDummy badeline, bool silent = false) {
            if (!silent) {
                Audio.Play("event:/char/badeline/disappear", badeline.Position);
            }

            SceneAs<Level>().Displacement.AddBurst(badeline.Center, 0.5f, 24f, 96f, 0.4f);
            SceneAs<Level>().Particles.Emit(BadelineOldsite.P_Vanish, 12, badeline.Center, Vector2.One * 6f);
            badeline.RemoveSelf();
        }

        private void RemoveBadeline(BadelineDummy badeline) {
            if (respawnTimer <= 0) {
                respawnTimer = respwanTime;
            }
            Disappear(badeline);
            badelines.Remove(badeline);
        }

        public override void Removed(Scene scene) {
            badelines.ForEach(badeline => Disappear(badeline, badelines.IndexOf(badeline) != 0));
            base.Removed(scene);
        }
    }
}