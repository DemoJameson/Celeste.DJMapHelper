using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.DJMapHelper.Entities {
    [Pooled]
    [Tracked(false)]
    public class SeekerBossBeam : Entity {
        public const float ChargeTime = 1.4f;
        public const float FollowTime = 0.9f;
        public const float ActiveTime = 0.12f;
        private const float AngleStartOffset = 100f;
        private const float RotationSpeed = 200f;
        private const float CollideCheckSep = 2f;
        private const float BeamLength = 2000f;
        private const float BeamStartDist = 12f;
        private const int BeamsDrawn = 15;
        private const float SideDarknessAlpha = 0.35f;
        private readonly Sprite beamSprite;
        private readonly Sprite beamStartSprite;
        private readonly VertexPositionColor[] fade = new VertexPositionColor[24];
        private float activeTimer;
        private float angle;
        private float beamAlpha;
        private SeekerBoss boss;
        private float chargeTimer;
        private float followTimer;
        private Player player;
        private float sideFadeAlpha;

        public SeekerBossBeam() {
            Add(beamSprite = GFX.SpriteBank.Create("badeline_beam"));
            beamSprite.OnLastFrame = anim => {
                if (!(anim == "shoot")) {
                    return;
                }

                Destroy();
            };
            Add(beamStartSprite = GFX.SpriteBank.Create("badeline_beam_start"));
            beamSprite.Visible = false;
            Depth = -1000000;
        }

        public SeekerBossBeam Init(SeekerBoss boss, Player target) {
            this.boss = boss;
            chargeTimer = 1.4f;
            followTimer = 0.9f;
            activeTimer = 0.12f;
            beamSprite.Play("charge", false, false);
            sideFadeAlpha = 0.0f;
            beamAlpha = 0.0f;
            var num = (double) target.Y > (double) boss.Y + 16.0 ? -1 : 1;
            if (target.X >= (double) boss.X) {
                num *= -1;
            }

            angle = Calc.Angle(boss.BeamOrigin, target.Center);
            Vector2 to =
                Calc.ClosestPointOnLine(boss.BeamOrigin, boss.BeamOrigin + Calc.AngleToVector(angle, 2000f),
                    target.Center) + (target.Center - boss.BeamOrigin).Perpendicular().SafeNormalize(100f) * num;
            angle = Calc.Angle(boss.BeamOrigin, to);
            return this;
        }

        public override void Update() {
            base.Update();
            player = Scene.Tracker.GetEntity<Player>();
            beamAlpha = Calc.Approach(beamAlpha, 1f, 2f * Engine.DeltaTime);
            if (chargeTimer > 0.0) {
                sideFadeAlpha = Calc.Approach(sideFadeAlpha, 1f, Engine.DeltaTime);
                if (player == null || player.Dead) {
                    return;
                }

                followTimer -= Engine.DeltaTime;
                chargeTimer -= Engine.DeltaTime;
                if (followTimer > 0.0 && player.Center != boss.BeamOrigin) {
                    angle = Calc.Angle(boss.BeamOrigin,
                        Calc.Approach(
                            Calc.ClosestPointOnLine(boss.BeamOrigin, boss.BeamOrigin + Calc.AngleToVector(angle, 2000f),
                                player.Center), player.Center, 200f * Engine.DeltaTime));
                }
                else if (beamSprite.CurrentAnimationID == "charge") {
                    beamSprite.Play("lock", false, false);
                }

                if (chargeTimer <= 0.0) {
                    SceneAs<Level>().DirectionalShake(Calc.AngleToVector(angle, 1f), 0.15f);
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                    DissipateParticles();
                }
            }
            else {
                if (activeTimer <= 0.0) {
                    return;
                }

                sideFadeAlpha = Calc.Approach(sideFadeAlpha, 0.0f, Engine.DeltaTime * 8f);
                if (beamSprite.CurrentAnimationID != "shoot") {
                    beamSprite.Play("shoot", false, false);
                    beamStartSprite.Play("shoot", true, false);
                }

                activeTimer -= Engine.DeltaTime;
                if (activeTimer > 0.0) {
                    PlayerCollideCheck();
                }
            }
        }

        private void DissipateParticles() {
            Level level = SceneAs<Level>();
            Vector2 closestTo = level.Camera.Position + new Vector2(160f, 90f);
            Vector2 lineA = boss.BeamOrigin + Calc.AngleToVector(angle, 12f);
            Vector2 lineB = boss.BeamOrigin + Calc.AngleToVector(angle, 2000f);
            Vector2 vector = (lineB - lineA).Perpendicular().SafeNormalize();
            Vector2 vector2_1 = (lineB - lineA).SafeNormalize();
            Vector2 min = -vector * 1f;
            Vector2 max = vector * 1f;
            var direction1 = vector.Angle();
            var direction2 = (-vector).Angle();
            var num = Vector2.Distance(closestTo, lineA) - 12f;
            Vector2 vector2_2 = Calc.ClosestPointOnLine(lineA, lineB, closestTo);
            for (var index1 = 0; index1 < 200; index1 += 12)
            for (var index2 = -1; index2 <= 1; index2 += 2) {
                level.ParticlesFG.Emit(FinalBossBeam.P_Dissipate,
                    vector2_2 + vector2_1 * index1 + vector * 2f * index2 + Calc.Random.Range(min, max), direction1);
                level.ParticlesFG.Emit(FinalBossBeam.P_Dissipate,
                    vector2_2 + vector2_1 * index1 - vector * 2f * index2 + Calc.Random.Range(min, max), direction2);
                if (index1 != 0 && index1 < (double) num) {
                    level.ParticlesFG.Emit(FinalBossBeam.P_Dissipate,
                        vector2_2 - vector2_1 * index1 + vector * 2f * index2 + Calc.Random.Range(min, max),
                        direction1);
                    level.ParticlesFG.Emit(FinalBossBeam.P_Dissipate,
                        vector2_2 - vector2_1 * index1 - vector * 2f * index2 + Calc.Random.Range(min, max),
                        direction2);
                }
            }
        }

        private void PlayerCollideCheck() {
            Vector2 from = boss.BeamOrigin + Calc.AngleToVector(angle, 12f);
            Vector2 to = boss.BeamOrigin + Calc.AngleToVector(angle, 2000f);
            Vector2 vector2 = (to - from).Perpendicular().SafeNormalize(2f);
            Player player =
                (Scene.CollideFirst<Player>(from + vector2, to + vector2) ??
                 Scene.CollideFirst<Player>(from - vector2, to - vector2)) ?? Scene.CollideFirst<Player>(from, to);
            if (player == null) {
                return;
            }

            player.Die((player.Center - boss.BeamOrigin).SafeNormalize(), false, true);
        }

        public override void Render() {
            Vector2 beamOrigin = boss.BeamOrigin;
            Vector2 vector1 = Calc.AngleToVector(angle, beamSprite.Width);
            beamSprite.Rotation = angle;
            beamSprite.Color = Color.White * beamAlpha;
            beamStartSprite.Rotation = angle;
            beamStartSprite.Color = Color.White * beamAlpha;
            if (beamSprite.CurrentAnimationID == "shoot") {
                beamOrigin += Calc.AngleToVector(angle, 8f);
            }

            for (var index = 0; index < 15; ++index) {
                beamSprite.RenderPosition = beamOrigin;
                beamSprite.Render();
                beamOrigin += vector1;
            }

            if (beamSprite.CurrentAnimationID == "shoot") {
                beamStartSprite.RenderPosition = boss.BeamOrigin;
                beamStartSprite.Render();
            }

            GameplayRenderer.End();
            Vector2 vector2 = vector1.SafeNormalize();
            Vector2 vector2_1 = vector2.Perpendicular();
            Color color = Color.Black * sideFadeAlpha * 0.35f;
            Color transparent = Color.Transparent;
            Vector2 vector2_2 = vector2 * 4000f;
            Vector2 vector2_3 = vector2_1 * 120f;
            var v = 0;
            Quad(ref v, beamOrigin, -vector2_2 + vector2_3 * 2f, vector2_2 + vector2_3 * 2f, vector2_2 + vector2_3,
                -vector2_2 + vector2_3, color, color);
            Quad(ref v, beamOrigin, -vector2_2 + vector2_3, vector2_2 + vector2_3, vector2_2, -vector2_2, color,
                transparent);
            Quad(ref v, beamOrigin, -vector2_2, vector2_2, vector2_2 - vector2_3, -vector2_2 - vector2_3, transparent,
                color);
            Quad(ref v, beamOrigin, -vector2_2 - vector2_3, vector2_2 - vector2_3, vector2_2 - vector2_3 * 2f,
                -vector2_2 - vector2_3 * 2f, color, color);
            GFX.DrawVertices((Scene as Level).Camera.Matrix, fade, fade.Length, null, null);
            GameplayRenderer.Begin();
        }

        private void Quad(
            ref int v,
            Vector2 offset,
            Vector2 a,
            Vector2 b,
            Vector2 c,
            Vector2 d,
            Color ab,
            Color cd) {
            fade[v].Position.X = offset.X + a.X;
            fade[v].Position.Y = offset.Y + a.Y;
            fade[v++].Color = ab;
            fade[v].Position.X = offset.X + b.X;
            fade[v].Position.Y = offset.Y + b.Y;
            fade[v++].Color = ab;
            fade[v].Position.X = offset.X + c.X;
            fade[v].Position.Y = offset.Y + c.Y;
            fade[v++].Color = cd;
            fade[v].Position.X = offset.X + a.X;
            fade[v].Position.Y = offset.Y + a.Y;
            fade[v++].Color = ab;
            fade[v].Position.X = offset.X + c.X;
            fade[v].Position.Y = offset.Y + c.Y;
            fade[v++].Color = cd;
            fade[v].Position.X = offset.X + d.X;
            fade[v].Position.Y = offset.Y + d.Y;
            fade[v++].Color = cd;
        }

        public void Destroy() {
            RemoveSelf();
        }
    }
}