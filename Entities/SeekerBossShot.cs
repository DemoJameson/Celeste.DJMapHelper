using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DJMapHelper.Entities {
    [Pooled]
    [Tracked(false)]
    public class SeekerBossShot : Entity {
        public enum ShotPatterns {
            Single,
            Double,
            Triple
        }

        private const float MoveSpeed = 100f;
        private const float CantKillTime = 0.15f;
        private const float AppearTime = 0.1f;
        private readonly SineWave sine;
        private readonly Sprite sprite;
        private Vector2 anchor;
        private float angleOffset;
        private float appearTimer;
        private SeekerBoss boss;
        private float cantKillTimer;
        private bool dead;
        private bool hasBeenInCamera;
        private Level level;
        private float particleDir;
        private Vector2 perp;
        private float sineMult;
        private Vector2 speed;
        private Player target;
        private Vector2 targetPt;

        public SeekerBossShot() : base(Vector2.Zero) {
            Add(sprite = GFX.SpriteBank.Create("badeline_projectile"));
            Collider = new Hitbox(4f, 4f, -2f, -2f);
            Add(new PlayerCollider(OnPlayer, null, null));
            Depth = -1000000;
            Add(sine = new SineWave(1.4f, 0.0f));
        }

        public SeekerBossShot Init(SeekerBoss boss, Player target, float angleOffset = 0.0f) {
            this.boss = boss;
            anchor = Position = boss.Center;
            this.target = target;
            this.angleOffset = angleOffset;
            dead = hasBeenInCamera = false;
            cantKillTimer = 0.15f;
            appearTimer = 0.1f;
            sine.Reset();
            sineMult = 0.0f;
            sprite.Play("charge", true, false);
            InitSpeed();
            return this;
        }

        public SeekerBossShot Init(SeekerBoss boss, Vector2 target) {
            this.boss = boss;
            anchor = Position = boss.Center;
            this.target = null;
            angleOffset = 0.0f;
            targetPt = target;
            dead = hasBeenInCamera = false;
            cantKillTimer = 0.15f;
            appearTimer = 0.1f;
            sine.Reset();
            sineMult = 0.0f;
            sprite.Play("charge", true, false);
            InitSpeed();
            return this;
        }

        private void InitSpeed() {
            speed = target == null
                ? (targetPt - Center).SafeNormalize(100f)
                : (target.Center - Center).SafeNormalize(100f);
            if (angleOffset != 0.0)
                speed = speed.Rotate(angleOffset);
            perp = speed.Perpendicular().SafeNormalize();
            particleDir = (-speed).Angle();
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            level = SceneAs<Level>();
        }

        public override void Removed(Scene scene) {
            base.Removed(scene);
            level = null;
        }

        public override void Update() {
            base.Update();
            if (appearTimer > 0.0) {
                Position = anchor = boss.ShotOrigin;
                appearTimer -= Engine.DeltaTime;
            }
            else {
                if (cantKillTimer > 0.0)
                    cantKillTimer -= Engine.DeltaTime;
                anchor += speed * Engine.DeltaTime;
                Position = anchor + perp * sineMult * sine.Value * 3f;
                sineMult = Calc.Approach(sineMult, 1f, 2f * Engine.DeltaTime);
                if (!dead) {
                    var flag = level.IsInCamera(Position, 8f);
                    if (flag && !hasBeenInCamera)
                        hasBeenInCamera = true;
                    else if (!flag && hasBeenInCamera)
                        Destroy();
                    if (Scene.OnInterval(0.04f))
                        level.ParticlesFG.Emit(FinalBossShot.P_Trail, 1, Center, Vector2.One * 2f, particleDir);
                }
            }
        }

        public override void Render() {
            Color color = sprite.Color;
            Vector2 position = sprite.Position;
            sprite.Color = Color.Black;
            sprite.Position = position + new Vector2(-1f, 0.0f);
            sprite.Render();
            sprite.Position = position + new Vector2(1f, 0.0f);
            sprite.Render();
            sprite.Position = position + new Vector2(0.0f, -1f);
            sprite.Render();
            sprite.Position = position + new Vector2(0.0f, 1f);
            sprite.Render();
            sprite.Color = color;
            sprite.Position = position;
            base.Render();
        }

        public void Destroy() {
            dead = true;
            RemoveSelf();
        }

        private void OnPlayer(Player player) {
            if (dead)
                return;
            if (cantKillTimer > 0.0)
                Destroy();
            else
                player.Die((player.Center - Position).SafeNormalize(), false, true);
        }
    }
}