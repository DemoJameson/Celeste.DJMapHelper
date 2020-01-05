using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DJMapHelper.Entities {
    [Tracked(false)]
    public class SeekerBoss : Actor {
        private const int size = 12;
        private const int bounceWidth = 16;
        private const int bounceHeight = 4;
        private const float Accel = 600f;
        private const float WallCollideStunThreshold = 100f;
        private const float StunXSpeed = 100f;
        private const float BounceSpeed = 200f;
        private const float SightDistSq = 25600f;
        private const float ExplodeRadius = 40f;
        private const float FarDistSq = 12544f;
        private const float IdleAccel = 200f;
        private const float IdleSpeed = 50f;
        private const float SpottedTargetSpeed = 60f;
        private const float SpottedFarSpeed = 90f;
        private const float SpottedMaxYDist = 24f;
        private const float AttackMinXDist = 16f;
        private const float AttackWindUpSpeed = -60f;
        private const float AttackWindUpTime = 0.3f;
        private const float AttackStartSpeed = 180f;
        private const float AttackTargetSpeed = 260f;
        private const float AttackAccel = 300f;
        private const float DirectionDotThreshold = 0.4f;
        private const int AttackTargetUpShift = 2;
        private const float AttackMaxRotateRadians = 0.6108652f;
        private const float StunnedAccel = 150f;
        private const float StunTime = 0.8f;
        private const float SkiddingAccel = 200f;
        private const float StrongSkiddingAccel = 400f;

        private const float StrongSkiddingTime = 0.08f;

        //我只需要两个状态，闲暇和复活
        private const int stateIdle = 0;
        private const int stateRegenerate = 1;
        private const int FinalPattern = 6;
        public static readonly Color TrailColor = Calc.HexToColor("99e550");
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
        private readonly Hitbox physicsHitbox;
        private readonly Circle pushRadius;
        private readonly SoundSource reviveSfx;
        private readonly Wiggler scaleWiggler;
        private readonly Shaker shaker;

        //新的变量
        private readonly Vector2 spinnerps1;
        private readonly Vector2 spinnerps2;
        private readonly Vector2 spinnerps3;
        private readonly Vector2 spinnerps4;
        private readonly Vector2 spinnerps5;
        private readonly Vector2 spinnerps6;
        private readonly Sprite sprite;
        private readonly StateMachine State;
        private readonly Vector2 switchps1;
        private readonly Vector2 switchps2;
        private readonly Vector2 switchps3;
        private readonly Vector2 switchps4;
        private SoundSource aggroSfx;
        private int facing = 1;
        private float gliderRespawnTime;
        private bool gotProtected;
        private Vector2 lastPosition;
        public VertexLight Light;
        private string nextSprite;
        private List<Vector2> path;
        private int pattern;
        private Random random;
        public Vector2 Speed;
        private int spriteFacing = 1;
        private float switchAppearTime;

        public SeekerBoss(Vector2 position) : base(position) {
            Depth = -200;
            Collider = physicsHitbox = new Hitbox(6f, 6f, -3f, -3f);
            attackHitbox = new Hitbox(12f, 8f, -6f, -2f);
            bounceHitbox = new Hitbox(16f, 6f, -8f, -8f);
            pushRadius = new Circle(40f, 0.0f, 0.0f);
            Add(new PlayerCollider(OnAttackPlayer, attackHitbox, null));
            Add(new PlayerCollider(OnBouncePlayer, bounceHitbox, null));
            Add(shaker = new Shaker(false, null));
            Add(State = new StateMachine(10));
            State.SetCallbacks(0, IdleUpdate, null, null, null);
            State.SetCallbacks(1, RegenerateUpdate, RegenerateCoroutine, RegenerateBegin, RegenerateEnd);
            Add(idleSineX = new SineWave(0.5f, 0.0f));
            Add(idleSineY = new SineWave(0.7f, 0.0f));
            Add(Light = new VertexLight(Color.White, 1f, 32, 64));
            Add(new MirrorReflection());
            path = new List<Vector2>();
            IgnoreJumpThrus = true;
            Add(sprite = GFX.SpriteBank.Create("seeker"));
            sprite.OnLastFrame = f => {
                if (!flipAnimations.Contains(f) || spriteFacing == facing)
                    return;
                spriteFacing = facing;
                if (nextSprite != null) {
                    sprite.Play(nextSprite, false, false);
                    nextSprite = null;
                }
            };
            sprite.OnChange = (last, next) => {
                nextSprite = null;
                sprite.OnLastFrame(last);
            };
            scaleWiggler = Wiggler.Create(0.8f, 2f, null, false, false);
            Add(scaleWiggler);
            Add(boopedSfx = new SoundSource());
            Add(aggroSfx = new SoundSource());
            Add(reviveSfx = new SoundSource());
            //原码到此
            spinnerps1 = position + new Vector2(0, 16);
            spinnerps2 = position + new Vector2(0, -16);
            spinnerps3 = position + new Vector2(16, 8);
            spinnerps4 = position + new Vector2(16, -8);
            spinnerps5 = position + new Vector2(-16, 8);
            spinnerps6 = position + new Vector2(-16, -8);
            switchps1 = position + new Vector2(-128, -64);
            switchps2 = position + new Vector2(-88, 64);
            switchps3 = position + new Vector2(128, -64);
            switchps4 = position + new Vector2(88, 64);
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

        public Vector2 BeamOrigin => Center + sprite.Position + new Vector2(0.0f, -14f);

        public Vector2 ShotOrigin => Center + sprite.Position + new Vector2(6f * sprite.Scale.X, 2f);

        public bool Regenerating => State.State == 1;

        public void Die() {
            Entity entity = new Entity(Position);
            entity.Add(new DeathEffect(Color.Black, Center - Position) {
                OnEnd = () => entity.RemoveSelf()
            });
            entity.Depth = -1000000;
            Scene.Add(entity);
            Audio.Play("event:/game/05_mirror_temple/seeker_death", Position);

            //死亡的后续代码
            foreach (InvisibleBarrier barrier in Scene.Tracker.GetEntities<InvisibleBarrier>())
                barrier.RemoveSelf();
            foreach (TempleGate gate in Scene.Tracker.GetEntities<TempleGate>())
                gate.Open();
            foreach (BadelineProtector protector in Scene.Entities.FindAll<BadelineProtector>())
                if (protector != null) {
                    BadelineDummy badeline = protector.badeline;
                    if (badeline.Visible)
                        badeline.Vanish();
                    else badeline.RemoveSelf();
                    protector.RemoveSelf();
                }

            Audio.SetMusic(null, true, true);
            RemoveSelf();
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            random = new Random(SceneAs<Level>().Session.LevelData.LoadSeed);
            //需要生成保护刺
            GenerateSpinner();
            GenerateCoin();
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity == null || X == (double) entity.X)
                SnapFacing(1f);
            else
                SnapFacing(Math.Sign(entity.X - X));

            //检查钥匙
            Add(new Coroutine(CheckTouchSwitches(), true));
        }

        public override bool IsRiding(JumpThru jumpThru) {
            return false;
        }

        public override bool IsRiding(Solid solid) {
            return false;
        }

        private void OnAttackPlayer(Player player) {
            player.Die((player.Center - Position).SafeNormalize(), false, true);
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
                State.State = 1;
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
            Light.Alpha = Calc.Approach(Light.Alpha, 1f, Engine.DeltaTime * 2f);
            sprite.Scale.X = Calc.Approach(sprite.Scale.X, 1f, 2f * Engine.DeltaTime);
            sprite.Scale.Y = Calc.Approach(sprite.Scale.Y, 1f, 2f * Engine.DeltaTime);
            base.Update();
            lastPosition = Position;
            MoveH(Speed.X * Engine.DeltaTime, null, null);
            MoveV(Speed.Y * Engine.DeltaTime, null, null);
            bounceHitbox.Width = 12f;
            bounceHitbox.Position.X = -6f;
        }

        private void TurnFacing(float dir, string gotoSprite = null) {
            if (dir != 0.0)
                facing = Math.Sign(dir);
            if (spriteFacing != facing) {
                sprite.Play("flipEyes", false, false);
                nextSprite = gotoSprite;
            }
            else {
                if (gotoSprite == null)
                    return;
                sprite.Play(gotoSprite, false, false);
            }
        }

        private void SnapFacing(float dir) {
            if (dir == 0.0)
                return;
            spriteFacing = facing = Math.Sign(dir);
        }

        public override void Render() {
            Vector2 position = Position;
            Position = Position + shaker.Value;
            Vector2 scale = this.sprite.Scale;
            Sprite sprite = this.sprite;
            sprite.Scale = sprite.Scale * (float) (1.0 - 0.300000011920929 * scaleWiggler.Value);
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
            Speed = Calc.Approach(Speed, target, 200f * Engine.DeltaTime);
            if (Speed.LengthSquared() > 400.0)
                TurnFacing(Speed.X, null);
            if (spriteFacing == facing)
                sprite.Play("idle", false, false);
            //往这里开始插攻击代码
            //是否生成水母
            if (gliderRespawnTime > 0.0) {
                gliderRespawnTime -= Engine.DeltaTime;
                if (gliderRespawnTime <= 0.0) GenerateGlider();
            }
            else if (Scene.Entities.FindFirst<Glider>() == null) {
                gliderRespawnTime = 7f;
            }

            //是否放钥匙
            if (switchAppearTime > 0.0f) {
                switchAppearTime -= Engine.DeltaTime;
                if (switchAppearTime <= 0.0) GenerateCoin();
            }

            return 0;
        }

        private void RegenerateBegin() {
            Audio.Play("event:/game/general/thing_booped", Position);
            boopedSfx.Play("event:/game/05_mirror_temple/seeker_booped", null, 0.0f);
            sprite.Play("takeHit", false, false);
            Collidable = false;
            State.Locked = true;
            Light.StartRadius = 16f;
            Light.EndRadius = 32f;

            //消除障碍
            foreach (TouchSwitch entity in Scene.Tracker.GetEntities<TouchSwitch>())
                entity.RemoveSelf();
            foreach (SeekerBossShot entity in Scene.Tracker.GetEntities<SeekerBossShot>())
                entity.Destroy();
            foreach (SeekerBossBeam entity in Scene.Tracker.GetEntities<SeekerBossBeam>())
                entity.Destroy();
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
            reviveSfx.Play("event:/game/05_mirror_temple/seeker_revive", null, 0.0f);
            Collidable = true;
            Light.StartRadius = 32f;
            Light.EndRadius = 64f;

            //这里写复活代码
            pattern = pattern + 1;
            switchAppearTime = 15f;
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
                        new Vector2[1] {Position + new Vector2(64f, 0)}));
                    Scene.Add(new Seeker(Position + new Vector2(-40f, 0f),
                        new Vector2[1] {Position + new Vector2(-64f, 0)}));
                    break;
                case 4:
                    attackCoroutine.Replace(Attack4Sequence());
                    Player player = Scene.Tracker.GetEntity<Player>();
                    if (player != null)
                        player.Visible = false;
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
            Speed.X = Calc.Approach(Speed.X, 0.0f, 150f * Engine.DeltaTime);
            Speed = Calc.Approach(Speed, Vector2.Zero, 150f * Engine.DeltaTime);
            return 1;
        }

        private IEnumerator RegenerateCoroutine() {
            yield return 1f;
            shaker.On = true;
            yield return 0.2f;
            sprite.Play("pulse", false, false);
            yield return 0.5f;
            sprite.Play("recover", false, false);
            RecoverBlast.Spawn(Position);
            yield return 0.15f;
            Collider = pushRadius;
            Player player = CollideFirst<Player>();
            if (player != null && !Scene.CollideCheck<Solid>(Position, player.Center))
                player.ExplodeLaunch(Position, true, false);
            Collider = physicsHitbox;
            Level level = SceneAs<Level>();
            level.Displacement.AddBurst(Position, 0.4f, 12f, 36f, 0.5f, null, null);
            level.Displacement.AddBurst(Position, 0.4f, 24f, 48f, 0.5f, null, null);
            level.Displacement.AddBurst(Position, 0.4f, 36f, 60f, 0.5f, null, null);
            for (var i = 0.0f; (double) i < 6.28318548202515; i += 0.1745329f) {
                Vector2 at = Center + Calc.AngleToVector(
                                 i + Calc.Random.Range(-1f * (float) Math.PI / 90f, (float) Math.PI / 90f),
                                 Calc.Random.Range(12, 18));
                level.Particles.Emit(Seeker.P_Regen, at, i);
                at = new Vector2();
            }

            player = null;
            level = null;
            shaker.On = false;
            State.Locked = false;
            State.State = 0;
        }

        private IEnumerator Attack0Sequence() {
            yield break;
        }

        private IEnumerator Attack1Sequence() {
            yield return 0.3f;
            while (true) {
                Shoot(0.0f);
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
                Shoot(0.0f);
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
                Shoot(0.0f);
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
            Scene.Add(new CrystalStaticSpinner(spinnerps1, false, CrystalColor.Red));
            Scene.Add(new CrystalStaticSpinner(spinnerps2, false, CrystalColor.Red));
            Scene.Add(new CrystalStaticSpinner(spinnerps3, false, CrystalColor.Red));
            Scene.Add(new CrystalStaticSpinner(spinnerps4, false, CrystalColor.Red));
            Scene.Add(new CrystalStaticSpinner(spinnerps5, false, CrystalColor.Red));
            Scene.Add(new CrystalStaticSpinner(spinnerps6, false, CrystalColor.Red));
            gotProtected = true;
        }

        private void DestroySpinner() {
            foreach (CrystalStaticSpinner entity in Scene.Tracker.GetEntities<CrystalStaticSpinner>())
                if (entity.Position == spinnerps1)
                    entity.Destroy();
                else if (entity.Position == spinnerps2)
                    entity.Destroy();
                else if (entity.Position == spinnerps3)
                    entity.Destroy();
                else if (entity.Position == spinnerps4)
                    entity.Destroy();
                else if (entity.Position == spinnerps5)
                    entity.Destroy();
                else if (entity.Position == spinnerps6)
                    entity.Destroy();
            gotProtected = false;
        }

        private void GenerateGlider() {
            Audio.Play("event:/new_content/game/10_farewell/glider_emancipate", gliderPosition);
            Scene.Add(new Glider(gliderPosition, true, false));
        }

        private void GenerateCoin() {
            Audio.Play("event:/new_content/char/badeline/booster_first_appear", Position);
            Scene.Add(new TouchSwitch(switchps1));
            Scene.Add(new TouchSwitch(switchps2));
            Scene.Add(new TouchSwitch(switchps3));
            Scene.Add(new TouchSwitch(switchps4));
        }

        private void ChangeWind(WindController.Patterns wind) {
            WindController first = Scene.Entities.FindFirst<WindController>();
            if (first == null)
                Scene.Add(new WindController(wind));
            else
                first.SetPattern(wind);
        }

        private IEnumerator CheckTouchSwitches() {
            while (true)
                if (!gotProtected) {
                    yield return null;
                }
                else {
                    while (!Switch.Check(Scene))
                        yield return null;
                    yield return 0.5f;
                    DestroySpinner();
                }
        }

        private void StartShootCharge() {
            chargeSfx.Play("event:/char/badeline/boss_bullet", null, 0.0f);
        }

        private void Shoot(float angleOffset = 0.0f) {
            if (!chargeSfx.Playing)
                chargeSfx.Play("event:/char/badeline/boss_bullet", "end", 1f);
            else
                chargeSfx.Param("end", 1f);
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity == null)
                return;
            Scene.Add(Engine.Pooler.Create<SeekerBossShot>().Init(this, entity, angleOffset));
        }

        private void ShootAt(Vector2 at) {
            if (!chargeSfx.Playing)
                chargeSfx.Play("event:/char/badeline/boss_bullet", "end", 1f);
            else
                chargeSfx.Param("end", 1f);
            Scene.Add(Engine.Pooler.Create<SeekerBossShot>().Init(this, at));
        }

        private IEnumerator Beam() {
            laserSfx.Play("event:/char/badeline/boss_laser_charge", null, 0.0f);
            yield return 0.1f;
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null)
                Scene.Add(Engine.Pooler.Create<SeekerBossBeam>().Init(this, player));
            yield return 0.9f;
            yield return 0.5f;
            laserSfx.Stop(true);
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

                sprite.Play("shockwave", true, false);
            }

            public static void Spawn(Vector2 position) {
                RecoverBlast recoverBlast = Engine.Pooler.Create<RecoverBlast>();
                recoverBlast.Position = position;
                Engine.Scene.Add(recoverBlast);
            }
        }
    }
}