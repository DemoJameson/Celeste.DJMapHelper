using System;
using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DJMapHelper.Entities {
    [Tracked]
    [CustomEntity("DJMapHelper/oshiroBossRight")]
    public class AngryOshiroRight : Entity {
        private const int StChase = 0;
        private const int StChargeUp = 1;
        private const int StAttack = 2;
        private const int StDummy = 3;
        private const int StWaiting = 4;
        private const int StHurt = 5;
        private const float HitboxBackRange = 4f;
        private const float minCameraOffsetX = -48f;
        private const float yApproachTargetSpeed = 100f;
        private const float HurtXSpeed = 100f;
        private const float HurtYSpeed = 200f;

        private static readonly float[] ChaseWaitTimes = new float[5] {
            1f,
            2f,
            3f,
            2f,
            3f
        };

        private readonly SoundSource chargeSfx;
        private readonly Vector2 colliderTargetPosition;
        private readonly Sprite lightning;
        private readonly SoundSource prechargeSfx;
        private readonly Shaker shaker;
        private readonly StateMachine state;

        private float anxietySpeed;
        private int attackIndex;
        private float attackSpeed;
        private PlayerCollider bounceCollider;
        private float cameraXOffset;
        private bool canControlTimeRate = true;
        private bool doRespawnAnim;
        private bool easeBackFromRightEdge;
        private bool hasEnteredSfx;
        private bool leaving;
        private Level level;
        private VertexLight light;
        private bool lightningVisible;
        private SineWave sine;
        public Sprite Sprite;
        private float targetAnxiety;
        private float yApproachSpeed = 100f;
        private bool fromCutscene;

        public AngryOshiroRight(Vector2 position, bool fromCutscene = false) : base(position) {
            Add(Sprite = GFX.SpriteBank.Create("oshiro_boss"));
            Sprite.Play("idle");
            Add(lightning = GFX.SpriteBank.Create("oshiro_boss_lightning"));
            lightning.Visible = false;
            lightning.OnFinish = s => lightningVisible = false;
            Collider = new Circle(14f);
            Collider.Position = colliderTargetPosition = new Vector2(-3f, 4f);
            Add(sine = new SineWave(0.5f));
            Add(bounceCollider = new PlayerCollider(OnPlayerBounce, new Hitbox(28f, 6f, -17f, -11f)));
            Add(new PlayerCollider(OnPlayer));
            Depth = -12500;
            Visible = false;
            Add(light = new VertexLight(Color.White, 1f, 32, 64));
            Add(shaker = new Shaker(false));
            state = new StateMachine();
            state.SetCallbacks(StChase, ChaseUpdate, ChaseCoroutine, ChaseBegin);
            state.SetCallbacks(StChargeUp, ChargeUpUpdate, ChargeUpCoroutine, null, ChargeUpEnd);
            state.SetCallbacks(StAttack, AttackUpdate, AttackCoroutine, AttackBegin, AttackEnd);
            state.SetCallbacks(StDummy, null);
            state.SetCallbacks(StWaiting, WaitingUpdate);
            state.SetCallbacks(StHurt, HurtUpdate, null, HurtBegin);
            Add(state);
            if (fromCutscene) {
                yApproachSpeed = 0f;
            }
            this.fromCutscene = fromCutscene;
            Add(new TransitionListener {
                OnOutBegin = () => {
                    if (X < level.Bounds.Right - Sprite.Width / 2.0) {
                        Visible = false;
                    } else {
                        easeBackFromRightEdge = true;
                    }
                },
                OnOut = f => {
                    lightning.Update();
                    if (!easeBackFromRightEdge) {
                        return;
                    }

                    X += 128f * Engine.RawDeltaTime;
                }
            });
            Add(prechargeSfx = new SoundSource());
            Add(chargeSfx = new SoundSource());
            Distort.AnxietyOrigin = new Vector2(1f, 0.5f);
            Sprite.Scale.X *= -1;
        }

        public AngryOshiroRight(EntityData data, Vector2 offset)
            : this(data.Position + offset + Vector2.UnitX * 10000, data.Bool("fromCutscene")) { }

        private float TargetY {
            get {
                Player entity = level.Tracker.GetEntity<Player>();
                if (entity != null) {
                    return MathHelper.Clamp(entity.CenterY, level.Bounds.Top + 8, level.Bounds.Bottom - 8);
                }

                return Y;
            }
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            level = SceneAs<Level>();
            if (level.Session.GetFlag("oshiroEnding") ||
                !level.Session.GetFlag("oshiro_resort_roof") && level.Session.Level.Equals("roof00")) {
                RemoveSelf();
            }

            if (state.State != StDummy) {
                state.State = StWaiting;
            }

            if (fromCutscene) {
                cameraXOffset = X - level.Camera.Right;
            } else {
                Y = TargetY;
                cameraXOffset = 48f;
            }
        }

        private void OnPlayer(Player player) {
            if (state.State == StHurt || CenterX <= player.CenterX - 4.0 && Sprite.CurrentAnimationID == "respawn") {
                return;
            }

            player.Die((player.Center - Center).SafeNormalize(Vector2.UnitX));
        }

        private void OnPlayerBounce(Player player) {
            if (state.State != StAttack || player.Bottom > Top + 6.0) {
                return;
            }

            Audio.Play("event:/game/general/thing_booped", Position);
            Celeste.Freeze(0.2f);
            player.Bounce(Top + 2f);
            state.State = StHurt;
            prechargeSfx.Stop();
            chargeSfx.Stop();
        }

        public override void Update() {
            base.Update();
            Sprite.Scale.X = Calc.Approach(Sprite.Scale.X, -1f, 0.6f * Engine.DeltaTime);
            Sprite.Scale.Y = Calc.Approach(Sprite.Scale.Y, 1f, 0.6f * Engine.DeltaTime);
            if (!doRespawnAnim) {
                Visible = X < level.Bounds.Right + Width / 2.0;
            }

            yApproachSpeed = Calc.Approach(yApproachSpeed, 100f, 300f * Engine.DeltaTime);
            if (state.State != 3 && canControlTimeRate) {
                if (state.State == 2 && attackSpeed > 200.0) {
                    Player player = Scene.Tracker.GetEntity<Player>();
                    Engine.TimeRate = player == null || player.Dead || CenterX <= player.CenterX - 4.0
                        ? 1f
                        : MathHelper.Lerp(Calc.ClampedMap(CenterX - player.CenterX, 30f, 80f, 0.5f), 1f,
                            Calc.ClampedMap(Math.Abs(player.CenterY - CenterY), 32f, 48f));
                } else {
                    Engine.TimeRate = 1f;
                }

                Distort.GameRate = Calc.Approach(Distort.GameRate, Calc.Map(Engine.TimeRate, 0.5f, 1f),
                    Engine.DeltaTime * 8f);
                Distort.Anxiety = Calc.Approach(Distort.Anxiety, targetAnxiety, anxietySpeed * Engine.DeltaTime);
            } else {
                Distort.GameRate = 1f;
                Distort.Anxiety = 0.0f;
            }
        }

        public void StopControllingTime() {
            canControlTimeRate = false;
        }

        public override void Render() {
            if (lightningVisible) {
                lightning.RenderPosition = new Vector2(level.Camera.Left - 2f, Top + 16f);
                lightning.Render();
            }

            Sprite.Position = shaker.Value * 2f;
            base.Render();
        }

        public void Leave() {
            leaving = true;
        }

        public void Squish() {
            //-1.3?
            Sprite.Scale = new Vector2(-1.3f, 0.5f);
            shaker.ShakeFor(0.5f, false);
        }

        private void ChaseBegin() {
            Sprite.Play("idle");
        }

        private int ChaseUpdate() {
            if (!hasEnteredSfx && cameraXOffset <= 16.0 && !doRespawnAnim) {
                Audio.Play("event:/char/oshiro/boss_enter_screen", Position);
                hasEnteredSfx = true;
            }

            if (doRespawnAnim && cameraXOffset <= 0.0) {
                Collider.Position.X = 48f;
                Visible = true;
                Sprite.Play("respawn");
                doRespawnAnim = false;
                if (Scene.Tracker.GetEntity<Player>() != null) {
                    Audio.Play("event:/char/oshiro/boss_reform", Position);
                }
            }

            cameraXOffset = Calc.Approach(cameraXOffset, -20f, 80f * Engine.DeltaTime);
            X = level.Camera.Right + cameraXOffset;
            Collider.Position.X = Calc.Approach(Collider.Position.X, colliderTargetPosition.X, Engine.DeltaTime * 128f);
            Collidable = Visible;
            if (level.Tracker.GetEntity<Player>() != null && Sprite.CurrentAnimationID != "respawn") {
                CenterY = Calc.Approach(CenterY, TargetY, yApproachSpeed * Engine.DeltaTime);
            }

            return StChase;
        }

        private IEnumerator ChaseCoroutine() {
            if ((uint) level.Session.Area.Mode > 0U) {
                yield return 1f;
            } else {
                yield return ChaseWaitTimes[attackIndex];
                ++attackIndex;
                attackIndex %= ChaseWaitTimes.Length;
            }

            prechargeSfx.Play("event:/char/oshiro/boss_precharge");
            Sprite.Play("charge");
            yield return 0.7f;
            if (Scene.Tracker.GetEntity<Player>() != null) {
                Alarm.Set(this, 0.216f, () => chargeSfx.Play("event:/char/oshiro/boss_charge"));
                state.State = StChargeUp;
            } else {
                Sprite.Play("idle");
            }
        }

        private int ChargeUpUpdate() {
            if (level.OnInterval(0.05f)) {
                Sprite.Position = Calc.Random.ShakeVector();
            }

            cameraXOffset = Calc.Approach(cameraXOffset, 0.0f, 40f * Engine.DeltaTime);
            X = level.Camera.Right + cameraXOffset;
            Player entity = level.Tracker.GetEntity<Player>();
            if (entity != null) {
                CenterY = Calc.Approach(CenterY,
                    MathHelper.Clamp(entity.CenterY, level.Bounds.Top + 8, level.Bounds.Bottom - 8),
                    30f * Engine.DeltaTime);
            }

            return StChargeUp;
        }

        private void ChargeUpEnd() {
            Sprite.Position = Vector2.Zero;
        }

        private IEnumerator ChargeUpCoroutine() {
            Celeste.Freeze(0.05f);
            Distort.Anxiety = 0.3f;
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            lightningVisible = true;
            lightning.Play("once", true);
            yield return 0.3f;
            Player player = Scene.Tracker.GetEntity<Player>();
            state.State = player == null ? StChase : StAttack;
        }

        private void AttackBegin() {
            attackSpeed = 0.0f;
            targetAnxiety = 0.3f;
            anxietySpeed = 4f;
            level.DirectionalShake(Vector2.UnitX);
        }

        private void AttackEnd() {
            targetAnxiety = 0.0f;
            anxietySpeed = 0.5f;
        }

        private int AttackUpdate() {
            X -= attackSpeed * Engine.DeltaTime;
            attackSpeed = Calc.Approach(attackSpeed, 500f, 2000f * Engine.DeltaTime);
            if (X <= level.Camera.Left - 48.0) {
                if (leaving) {
                    RemoveSelf();
                    return StAttack;
                }

                X = level.Camera.Right + 48f;
                cameraXOffset = 48f;
                doRespawnAnim = true;
                Visible = false;
                return StChase;
            }

            Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
            if (Scene.OnInterval(0.05f)) {
                TrailManager.Add(this, Color.Red * 0.6f, 0.5f);
            }

            return StAttack;
        }

        private IEnumerator AttackCoroutine() {
            yield return 0.1f;
            targetAnxiety = 0.0f;
            anxietySpeed = 0.5f;
        }

        private int WaitingUpdate() {
            Player player = Scene.Tracker.GetEntity<Player>();
            return player != null && player.Speed != Vector2.Zero &&
                   player.X < (double) (level.Bounds.Right - 48)
                ? StChase
                : StWaiting;
        }

        private void HurtBegin() {
            Sprite.Play("hurt", true);
        }

        private int HurtUpdate() {
            X -= 100f * Engine.DeltaTime;
            Y += 200f * Engine.DeltaTime;
            if (Top <= (double) (level.Bounds.Bottom + 20)) {
                return StHurt;
            }

            if (leaving) {
                RemoveSelf();
                return StHurt;
            }

            X = level.Camera.Right + 48f;
            cameraXOffset = 48f;
            doRespawnAnim = true;
            Visible = false;
            return StChase;
        }
    }
}