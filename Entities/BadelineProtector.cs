using System;
using System.Reflection;
using Celeste.Mod.DJMapHelper.Extensions;
using Microsoft.Xna.Framework;
using Monocle;

// ReSharper disable PossibleInvalidCastExceptionInForeachLoop

namespace Celeste.Mod.DJMapHelper.Entities {
    public class BadelineProtector : Entity {
        private const float RespwanTime = 8f;
        private const float length = 24f;
        private static readonly MethodInfo SeekerGotBounced = typeof(Seeker).GetPrivateMethod("GotBouncedOn");
        public readonly BadelineDummy Badeline;
        private Player player;
        private float respawnTimer;
        private float rotationPercent;

        public BadelineProtector() {
            respawnTimer = 0.0f;
            rotationPercent = 0.0f;
            Badeline = new BadelineDummy(Vector2.Zero);
            Badeline.Sprite.Play("fallSlow");
            Position = Badeline.Position;
            Collider = new Hitbox(8f, 9f, -4f, -11f);
            Badeline.Visible = false;
        }

        private float Angle => MathHelper.Lerp(3.141593f, -3.141593f, Easer(rotationPercent));

        private float Easer(float v) {
            return v;
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if (player == null) {
                RemoveSelf();
            }

            Scene.Add(Badeline);
            ResetPosition();
            Appear();
        }

        public void ResetPosition() {
            rotationPercent -= Engine.DeltaTime / 1.8f;
            ++rotationPercent;
            rotationPercent %= 1f;
            Badeline.Position = player.Center + Calc.AngleToVector(Angle, length);
            Badeline.Sprite.Scale.X = Math.Sign(Badeline.X - player.X);
            Position = Badeline.Position;
        }

        public override void Update() {
            base.Update();
            ResetPosition();
            if (respawnTimer > 0.0) {
                respawnTimer -= Engine.DeltaTime;
                if (respawnTimer <= 0.0) {
                    Appear();
                }
            }

            if (Badeline.Visible) {
                foreach (TouchSwitch entity in Scene.Tracker.GetEntities<TouchSwitch>()) {
                    if (CollideCheck(entity) && !entity.Switch.Activated) {
                        entity.TurnOn();
                        respawnTimer = RespwanTime;
                        Vanish();
                        break;
                    }
                }
                
                foreach (Seeker entity in Scene.Tracker.GetEntities<Seeker>()) {
                    if (CollideCheck(entity)) {
                        SeekerGotBounced.Invoke(entity, new object[] {this});
                        respawnTimer = RespwanTime;
                        Vanish();
                        break;
                    }
                }
                
                foreach (SeekerBossShot entity in Scene.Tracker.GetEntities<SeekerBossShot>()) {
                    if (CollideCheck(entity)) {
                        entity.RemoveSelf();
                        respawnTimer = RespwanTime;
                        Vanish();
                        break;
                    }
                }
                
                foreach (SeekerBoss entity in Scene.Tracker.GetEntities<SeekerBoss>()) {
                    if (CollideCheck(entity)) {
                        respawnTimer = RespwanTime;
                        Vanish();
                        break;
                    }
                }
            }
        }

        private void Vanish() {
            Audio.Play("event:/char/badeline/disappear", Badeline.Position);
            SceneAs<Level>().Displacement.AddBurst(Badeline.Center, 0.5f, 24f, 96f, 0.4f);
            SceneAs<Level>().Particles.Emit(BadelineOldsite.P_Vanish, 12, Badeline.Center, Vector2.One * 6f);
            Badeline.Visible = false;
        }

        private void Appear() {
            Audio.Play("event:/char/badeline/appear", Badeline.Position);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            SceneAs<Level>().Displacement.AddBurst(Badeline.Center, 0.5f, 24f, 96f, 0.4f);
            SceneAs<Level>().Particles.Emit(BadelineOldsite.P_Vanish, 12, Badeline.Center, Vector2.One * 6f);
            Badeline.Visible = true;
        }
    }
}