using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

// ReSharper disable PossibleInvalidCastExceptionInForeachLoop
// ReSharper disable IteratorNeverReturns

namespace Celeste.Mod.DJMapHelper.Entities {
    [Tracked]
    [CustomEntity("DJMapHelper/seekerBoss")]
    public class SeekerBoss : Actor {
        //我只需要两个状态，闲暇和复活
        private const int StateIdle = 0;
        private const int StateRegenerate = 1;

        private const int FinalPattern = 6;
        private readonly Coroutine attackCoroutine;
        private readonly Hitbox attackHitbox;
        private readonly SoundSource boopedSfx;
        private readonly Hitbox bounceHitbox;
        private readonly SoundSource chargeSfx;

        private readonly HashSet<string> flipAnimations = new HashSet<string> {
            "flipMouth",
            "flipEyes",
            "skid"
        };

        private readonly Vector2 gliderPosition;
        private readonly SineWave idleSineX;
        private readonly SineWave idleSineY;
        private readonly SoundSource laserSfx;

        private readonly VertexLight light;
        private readonly Hitbox physicsHitbox;
        private readonly Circle pushRadius;
        private readonly SoundSource reviveSfx;
        private readonly Wiggler scaleWiggler;
        private readonly Shaker shaker;

        //新的变量
        private readonly Vector2 spinnerPosition1;
        private readonly Vector2 spinnerPosition2;
        private readonly Vector2 spinnerPosition3;
        private readonly Vector2 spinnerPosition4;
        private readonly Vector2 spinnerPosition5;
        private readonly Vector2 spinnerPosition6;
        private readonly Sprite sprite;
        private readonly StateMachine state;
        private readonly Vector2 switchPosition1;
        private readonly Vector2 switchPosition2;
        private readonly Vector2 switchPosition3;
        private readonly Vector2 switchPosition4;
        private int facing = 1;
        private float gliderRespawnTime;
        private bool gotProtected;
        private string nextSprite;
        private int pattern;
        private Vector2 speed;
        private int spriteFacing = 1;
        private float switchAppearTime;

        public SeekerBoss(Vector2 position) : base(position) {
            Depth = -200;
            Collider = physicsHitbox = new Hitbox(6f, 6f, -3f, -3f);
            attackHitbox = new Hitbox(12f, 8f, -6f, -2f);
            bounceHitbox = new Hitbox(16f, 6f, -8f, -8f);
            pushRadius = new Circle(40f);
            Add(new PlayerCollider(OnAttackPlayer, attackHitbox));
            Add(new PlayerCollider(OnBouncePlayer, bounceHitbox));
            Add(shaker = new Shaker(false));
            Add(state = new StateMachine());
            state.SetCallbacks(StateIdle, IdleUpdate);
            state.SetCallbacks(StateRegenerate, RegenerateUpdate, RegenerateCoroutine, RegenerateBegin, RegenerateEnd);
            Add(idleSineX = new SineWave(0.5f, 0.0f));
            Add(idleSineY = new SineWave(0.7f, 0.0f));
            Add(light = new VertexLight(Color.White, 1f, 32, 64));
            Add(new MirrorReflection());
            IgnoreJumpThrus = true;
            Add(sprite = GFX.SpriteBank.Create("seeker"));
            sprite.OnLastFrame = f => {
                if (!flipAnimations.Contains(f) || spriteFacing == facing) {
                    return;
                }

                spriteFacing = facing;
                if (nextSprite != null) {
                    sprite.Play(nextSprite);
                    nextSprite = null;
                }
            };
            sprite.OnChange = (last, next) => {
                nextSprite = null;
                sprite.OnLastFrame(last);
            };
            scaleWiggler = Wiggler.Create(0.8f, 2f);
            Add(scaleWiggler);
            Add(boopedSfx = new SoundSource());
            Add(reviveSfx = new SoundSource());
            //原码到此
            spinnerPosition1 = position + new Vector2(0, 16);
            spinnerPosition2 = position + new Vector2(0, -16);
            spinnerPosition3 = position + new Vector2(16, 8);
            spinnerPosition4 = position + new Vector2(16, -8);
            spinnerPosition5 = position + new Vector2(-16, 8);
            spinnerPosition6 = position + new Vector2(-16, -8);
            switchPosition1 = position + new Vector2(-128, -64);
            switchPosition2 = position + new Vector2(-88, 64);
            switchPosition3 = position + new Vector2(128, -64);
            switchPosition4 = position + new Vector2(88, 64);
            gliderPosition = position + new Vector2(40, -32);
            Add(chargeSfx = new SoundSource());
            Add(laserSfx = new SoundSource());
            pattern = 0;
            gliderRespawnTime = 1.0f;
            switchAppearTime = 0.0f;
            attackCoroutine = new Coroutine(false);
            Add(attackCoroutine);
            attackCoroutine.Replace(Attack0Sequence());
        }

        public SeekerBoss(EntityData data, Vector2 offset) : this(data.Position + offset) { }

        public Vector2 BeamOrigin => Center + new Vector2(-2f, -1f);

        public Vector2 ShotOrigin => Center;

        private void Die() {
            Entity entity = new Entity(Position);
            entity.Add(new DeathEffect(Color.Black, Center - Position) {
                OnEnd = () => entity.RemoveSelf()
            });
            entity.Depth = -1000000;
            Scene.Add(entity);
            Audio.Play("event:/game/05_mirror_temple/seeker_death", Position);

            //死亡的后续代码
            foreach (InvisibleBarrier barrier in Scene.Tracker.GetEntities<InvisibleBarrier>()) {
                barrier.RemoveSelf();
            }

            foreach (TempleGate gate in Scene.Tracker.GetEntities<TempleGate>()) {
                gate.Open();
            }

            foreach (BadelineProtector protector in Scene.Entities.FindAll<BadelineProtector>()) {
                protector.RemoveSelf();
            }
            DJMapHelperModule.Session.BadelineProtectorConfig = null;
            DJMapHelperModule.Session.DefeatedBoss = true;

            Audio.SetMusic(null);
            RemoveSelf();
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            if (DJMapHelperModule.Session.DefeatedBoss) {
                foreach (InvisibleBarrier barrier in scene.Tracker.GetEntities<InvisibleBarrier>()) {
                    barrier.RemoveSelf();
                }

                foreach (TempleGate gate in scene.Tracker.GetEntities<TempleGate>()) {
                    gate.Open();
                }
                RemoveSelf();
                return;
            }

            //需要生成保护刺
            GenerateSpinner();
            GenerateCoin();

            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity == null || Math.Abs(X - (double) entity.X) < 0.01) {
                SnapFacing(1f);
            }
            else {
                SnapFacing(Math.Sign(entity.X - X));
            }

            //检查钥匙
            Add(new Coroutine(CheckTouchSwitches()));
        }

        public override bool IsRiding(JumpThru jumpThru) {
            return false;
        }

        public override bool IsRiding(Solid solid) {
            return false;
        }

        private void OnAttackPlayer(Player player) {
            player.Die((player.Center - Position).SafeNormalize());
        }

        private void OnBouncePlayer(Player player) {
            Collider collider = Collider;
            Collider = attackHitbox;
            if (CollideCheck(player)) {
                OnAttackPlayer(player);
            }
            else if (gotProtected) {
                OnAttackPlayer(player);
            }
            else {
                player.Bounce(Top);
                GotBouncedOn(player);
            }

            Collider = collider;
        }

        private void GotBouncedOn(Entity entity) {
            Celeste.Freeze(0.15f);
            if (pattern != FinalPattern) {
                state.State = 1;
                sprite.Scale = new Vector2(1.4f, 0.6f);
                attackCoroutine.Active = false;
                ChangeWind(WindController.Patterns.None);
                entity.Visible = true;
                SceneAs<Level>().Particles.Emit(Seeker.P_Stomp, 8, Center - Vector2.UnitY * 5f, new Vector2(6f, 3f));
            }
            else {
                Die();
            }
        }

        public override void Update() {
            light.Alpha = Calc.Approach(light.Alpha, 1f, Engine.DeltaTime * 2f);
            sprite.Scale.X = Calc.Approach(sprite.Scale.X, 1f, 2f * Engine.DeltaTime);
            sprite.Scale.Y = Calc.Approach(sprite.Scale.Y, 1f, 2f * Engine.DeltaTime);
            base.Update();

            MoveH(speed.X * Engine.DeltaTime);
            MoveV(speed.Y * Engine.DeltaTime);
            bounceHitbox.Width = 12f;
            bounceHitbox.Position.X = -6f;
        }

        private void TurnFacing(float dir, string gotoSprite = null) {
            if (Math.Abs(dir) > 0.01) {
                facing = Math.Sign(dir);
            }

            if (spriteFacing != facing) {
                sprite.Play("flipEyes");
                nextSprite = gotoSprite;
            }
            else {
                if (gotoSprite == null) {
                    return;
                }

                sprite.Play(gotoSprite);
            }
        }

        private void SnapFacing(float dir) {
            if (Math.Abs(dir) < 0.01) {
                return;
            }

            spriteFacing = facing = Math.Sign(dir);
        }

        public override void Render() {
            Vector2 position = Position;
            Position = Position + shaker.Value;
            Vector2 scale = this.sprite.Scale;
            Sprite sprite = this.sprite;
            sprite.Scale = sprite.Scale * (float) (1.0 - 0.3 * scaleWiggler.Value);
            this.sprite.Scale.X *= spriteFacing;
            base.Render();
            Position = position;
            this.sprite.Scale = scale;
        }

        public override void DebugRender(Camera camera) {
            Collider collider = Collider;
            Collider = attackHitbox;
            attackHitbox.Render(camera, Color.Red);
            Collider = bounceHitbox;
            bounceHitbox.Render(camera, Color.Aqua);
            Collider = collider;
        }

        private int IdleUpdate() {
            Vector2 target = Vector2.Zero;
            target.X = idleSineX.Value * 6f;
            target.Y = idleSineY.Value * 6f;
            speed = Calc.Approach(speed, target, 200f * Engine.DeltaTime);
            if (speed.LengthSquared() > 400.0) {
                TurnFacing(speed.X);
            }

            if (spriteFacing == facing) {
                sprite.Play("idle");
            }

            //往这里开始插攻击代码
            //是否生成水母
            if (gliderRespawnTime > 0.0) {
                gliderRespawnTime -= Engine.DeltaTime;
                if (gliderRespawnTime <= 0.0) {
                    GenerateGlider();
                }
            }
            else if (Scene.Entities.FindFirst<Glider>() == null) {
                gliderRespawnTime = 7f;
            }

            //是否放钥匙
            if (switchAppearTime > 0.0f) {
                switchAppearTime -= Engine.DeltaTime;
                if (switchAppearTime <= 0.0) {
                    GenerateCoin();
                }
            }

            return 0;
        }

        private void RegenerateBegin() {
            Audio.Play("event:/game/general/thing_booped", Position);
            boopedSfx.Play("event:/game/05_mirror_temple/seeker_booped");
            sprite.Play("takeHit");
            Collidable = false;
            state.Locked = true;
            light.StartRadius = 16f;
            light.EndRadius = 32f;

            //消除障碍
            foreach (TouchSwitch entity in Scene.Tracker.GetEntities<TouchSwitch>()) {
                entity.RemoveSelf();
            }

            foreach (SeekerBossShot entity in Scene.Tracker.GetEntities<SeekerBossShot>()) {
                entity.Destroy();
            }

            foreach (SeekerBossBeam entity in Scene.Tracker.GetEntities<SeekerBossBeam>()) {
                entity.Destroy();
            }

            foreach (Seeker seeker in Scene.Tracker.GetEntities<Seeker>()) {
                Entity entity = new Entity(seeker.Position);
                entity.Add(new DeathEffect(Color.HotPink, seeker.Center - seeker.Position) {
                    OnEnd = () => entity.RemoveSelf()
                });
                entity.Depth = -1000000;
                Scene.Add(entity);
                Audio.Play("event:/game/05_mirror_temple/seeker_death", seeker.Position);
                seeker.RemoveSelf();
            }
        }

        private void RegenerateEnd() {
            reviveSfx.Play("event:/game/05_mirror_temple/seeker_revive");
            Collidable = true;
            light.StartRadius = 32f;
            light.EndRadius = 64f;

            //这里写复活代码
            pattern = pattern + 1;
            switchAppearTime = 6f;
            pattern = FinalPattern;
            switch (pattern) {
                case 1:
                    attackCoroutine.Replace(Attack1Sequence());
                    break;
                case 2:
                    attackCoroutine.Replace(Attack2Sequence());
                    ChangeWind(WindController.Patterns.Down);
                    break;
                case 3:
                    attackCoroutine.Replace(Attack3Sequence());
                    Scene.Add(new Seeker(Position + new Vector2(40f, 0f),
                        new[] {Position + new Vector2(64f, 0)}));
                    Scene.Add(new Seeker(Position + new Vector2(-40f, 0f),
                        new[] {Position + new Vector2(-64f, 0)}));
                    break;
                case 4:
                    attackCoroutine.Replace(Attack4Sequence());
                    Player player = Scene.Tracker.GetEntity<Player>();
                    if (player != null) {
                        player.Visible = false;
                    }

                    ChangeWind(WindController.Patterns.Up);
                    break;
                case 5:
                    attackCoroutine.Replace(Attack5Sequence());
                    ChangeWind(WindController.Patterns.Alternating);
                    break;
                case 6:
                    attackCoroutine.Replace(Attack6Sequence());
                    break;
                default:
                    attackCoroutine.Replace(Attack0Sequence());
                    break;
            }

            GenerateSpinner();
        }

        private int RegenerateUpdate() {
            speed.X = Calc.Approach(speed.X, 0.0f, 150f * Engine.DeltaTime);
            speed = Calc.Approach(speed, Vector2.Zero, 150f * Engine.DeltaTime);
            return 1;
        }

        private IEnumerator RegenerateCoroutine() {
            yield return 1f;
            shaker.On = true;
            yield return 0.2f;
            sprite.Play("pulse");
            yield return 0.5f;
            sprite.Play("recover");
            RecoverBlast.Spawn(Position);
            yield return 0.15f;
            Collider = pushRadius;
            Player player = CollideFirst<Player>();
            if (player != null && !Scene.CollideCheck<Solid>(Position, player.Center)) {
                player.ExplodeLaunch(Position, true, false);
            }

            Collider = physicsHitbox;
            Level level = SceneAs<Level>();
            level.Displacement.AddBurst(Position, 0.4f, 12f, 36f, 0.5f);
            level.Displacement.AddBurst(Position, 0.4f, 24f, 48f, 0.5f);
            level.Displacement.AddBurst(Position, 0.4f, 36f, 60f, 0.5f);
            for (var i = 0.0f; (double) i < 6.28318548202515; i += 0.1745329f) {
                Vector2 at = Center + Calc.AngleToVector(
                                 i + Calc.Random.Range(-1f * (float) Math.PI / 90f, (float) Math.PI / 90f),
                                 Calc.Random.Range(12, 18));
                level.Particles.Emit(Seeker.P_Regen, at, i);
            }

            shaker.On = false;
            state.Locked = false;
            state.State = 0;
        }

        private IEnumerator Attack0Sequence() {
            yield break;
        }

        private IEnumerator Attack1Sequence() {
            yield return 0.3f;
            while (true) {
                Shoot();
                yield return 0.4f;
                StartShootCharge();
                yield return 0.4f;
            }
        }

        private IEnumerator Attack2Sequence() {
            yield return 0.3f;
            while (true) {
                yield return 0.3f;
                yield return Beam();
                yield return 0.3f;
            }
        }

        private IEnumerator Attack3Sequence() {
            yield break;
        }

        private IEnumerator Attack4Sequence() {
            yield return 0.3f;
            while (true) {
                Shoot();
                yield return 1.5f;
                StartShootCharge();
                yield return 1.5f;
            }
        }

        private IEnumerator Attack5Sequence() {
            yield return 0.3f;
            while (true) {
                yield return Beam();
                yield return 0.4f;
                StartShootCharge();
                yield return 0.3f;
                Shoot();
                yield return 0.5f;
            }
        }

        private IEnumerator Attack6Sequence() {
            yield return 0.3f;
            while (true) {
                Scene.Add(Engine.Pooler.Create<TempleBigEyeballShockwave>().Init(Center + new Vector2(168f, 0.0f)));
                yield return 1.5f;
            }
        }

        private void GenerateSpinner() {
            Scene.Add(new CrystalStaticSpinner(spinnerPosition1, false, CrystalColor.Red));
            Scene.Add(new CrystalStaticSpinner(spinnerPosition2, false, CrystalColor.Red));
            Scene.Add(new CrystalStaticSpinner(spinnerPosition3, false, CrystalColor.Red));
            Scene.Add(new CrystalStaticSpinner(spinnerPosition4, false, CrystalColor.Red));
            Scene.Add(new CrystalStaticSpinner(spinnerPosition5, false, CrystalColor.Red));
            Scene.Add(new CrystalStaticSpinner(spinnerPosition6, false, CrystalColor.Red));
            gotProtected = true;
        }

        private void DestroySpinner() {
            foreach (CrystalStaticSpinner entity in Scene.Tracker.GetEntities<CrystalStaticSpinner>()) {
                if (entity.Position == spinnerPosition1) {
                    entity.Destroy();
                }
                else if (entity.Position == spinnerPosition2) {
                    entity.Destroy();
                }
                else if (entity.Position == spinnerPosition3) {
                    entity.Destroy();
                }
                else if (entity.Position == spinnerPosition4) {
                    entity.Destroy();
                }
                else if (entity.Position == spinnerPosition5) {
                    entity.Destroy();
                }
                else if (entity.Position == spinnerPosition6) {
                    entity.Destroy();
                }
            }

            gotProtected = false;
        }

        private void GenerateGlider() {
            Audio.Play("event:/new_content/game/10_farewell/glider_emancipate", gliderPosition);
            Scene.Add(new Glider(gliderPosition, true, false));
        }

        private void GenerateCoin() {
            Audio.Play("event:/new_content/char/badeline/booster_first_appear", Position);
            Scene.Add(new TouchSwitch(switchPosition1));
            Scene.Add(new TouchSwitch(switchPosition2));
            Scene.Add(new TouchSwitch(switchPosition3));
            Scene.Add(new TouchSwitch(switchPosition4));
        }

        private void ChangeWind(WindController.Patterns wind) {
            WindController first = Scene.Entities.FindFirst<WindController>();
            if (first == null) {
                Scene.Add(new WindController(wind));
            }
            else {
                first.SetPattern(wind);
            }
        }

        private IEnumerator CheckTouchSwitches() {
            while (true) {
                if (!gotProtected) {
                    yield return null;
                }
                else {
                    while (!Switch.Check(Scene)) {
                        yield return null;
                    }

                    yield return 0.5f;
                    DestroySpinner();
                }
            }
        }

        private void StartShootCharge() {
            chargeSfx.Play("event:/char/badeline/boss_bullet");
        }

        private void Shoot(float angleOffset = 0.0f) {
            if (!chargeSfx.Playing) {
                chargeSfx.Play("event:/char/badeline/boss_bullet", "end", 1f);
            }
            else {
                chargeSfx.Param("end", 1f);
            }

            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity == null) {
                return;
            }

            Scene.Add(Engine.Pooler.Create<SeekerBossShot>().Init(this, entity, angleOffset));
        }

        private IEnumerator Beam() {
            laserSfx.Play("event:/char/badeline/boss_laser_charge");
            yield return 0.1f;
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null) {
                Scene.Add(Engine.Pooler.Create<SeekerBossBeam>().Init(this, player));
            }

            yield return 0.9f;
            yield return 0.5f;
            laserSfx.Stop();
            Audio.Play("event:/char/badeline/boss_laser_fire", Position);
        }

        [Pooled]
        private class RecoverBlast : Entity {
            private Sprite sprite;

            public override void Added(Scene scene) {
                base.Added(scene);
                Depth = -199;
                if (sprite == null) {
                    Add(sprite = GFX.SpriteBank.Create("seekerShockWave"));
                    sprite.OnLastFrame = a => RemoveSelf();
                }

                sprite.Play("shockwave", true);
            }

            public static void Spawn(Vector2 position) {
                RecoverBlast recoverBlast = Engine.Pooler.Create<RecoverBlast>();
                recoverBlast.Position = position;
                Engine.Scene.Add(recoverBlast);
            }
        }
    }
}