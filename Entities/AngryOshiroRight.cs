using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.DJMapHelper.Entities 
{
  [Tracked]
  public class AngryOshiroRight : Entity
  {
    private static readonly float[] ChaseWaitTimes = new float[5]
    {
      1f,
      2f,
      3f,
      2f,
      3f
    };
    private bool canControlTimeRate = true;
    private float yApproachSpeed = 100f;
    private const int StChase = 0;
    private const int StChargeUp = 1;
    private const int StAttack = 2;
    private const int StDummy = 3;
    private const int StWaiting = 4;
    private const int StHurt = 5;
    private const float HitboxBackRange = 4f;
    public Sprite Sprite;
    private Sprite lightning;
    private bool lightningVisible;
    private VertexLight light;
    private Level level;
    private SineWave sine;
    private float cameraXOffset;
    private StateMachine state;
    private int attackIndex;
    private float targetAnxiety;
    private float anxietySpeed;
    private bool easeBackFromRightEdge;
    private bool doRespawnAnim;
    private bool leaving;
    private Shaker shaker;
    private PlayerCollider bounceCollider;
    private Vector2 colliderTargetPosition;
    private SoundSource prechargeSfx;
    private SoundSource chargeSfx;
    private bool hasEnteredSfx;
    private const float minCameraOffsetX = -48f;
    private const float yApproachTargetSpeed = 100f;
    private float attackSpeed;
    private const float HurtXSpeed = 100f;
    private const float HurtYSpeed = 200f;

    public AngryOshiroRight(Vector2 position)
    :base(position)
    {
      Add((Component) (Sprite = GFX.SpriteBank.Create("oshiro_boss")));
      Sprite.Play("idle", false, false);
      Add((Component) (lightning = GFX.SpriteBank.Create("oshiro_boss_lightning")));
      lightning.Visible = false;
      lightning.OnFinish = (Action<string>) (s => lightningVisible = false);
      Collider = (Collider) new Circle(14f, 0.0f, 0.0f);
      Collider.Position = colliderTargetPosition = new Vector2(-3f, 4f);
      Add((Component) (sine = new SineWave(0.5f)));
      Add((Component) (bounceCollider = new PlayerCollider(new Action<Player>(OnPlayerBounce), (Collider) new Hitbox(28f, 6f, -17f, -11f), (Collider) null)));
      Add((Component) new PlayerCollider(new Action<Player>(OnPlayer), (Collider) null, (Collider) null));
      Depth = -12500;
      Visible = false;
      Add((Component) (light = new VertexLight(Color.White, 1f, 32, 64)));
      Add((Component) (shaker = new Shaker(false, (Action<Vector2>) null)));
      state = new StateMachine(10);
      state.SetCallbacks(0, new Func<int>(ChaseUpdate), new Func<IEnumerator>(ChaseCoroutine), new Action(ChaseBegin), (Action) null);
      state.SetCallbacks(1, new Func<int>(ChargeUpUpdate), new Func<IEnumerator>(ChargeUpCoroutine), (Action) null, new Action(ChargeUpEnd));
      state.SetCallbacks(2, new Func<int>(AttackUpdate), new Func<IEnumerator>(AttackCoroutine), new Action(AttackBegin), new Action(AttackEnd));
      state.SetCallbacks(3, (Func<int>) null, (Func<IEnumerator>) null, (Action) null, (Action) null);
      state.SetCallbacks(4, new Func<int>(WaitingUpdate), (Func<IEnumerator>) null, (Action) null, (Action) null);
      state.SetCallbacks(5, new Func<int>(HurtUpdate), (Func<IEnumerator>) null, new Action(HurtBegin), (Action) null);
      Add((Component) state);
      Add((Component) new TransitionListener()
      {
        OnOutBegin = (Action) (() =>
        {
          if ((double) X < (double) level.Bounds.Right - (double) Sprite.Width / 2.0)
            Visible = false;
          else
            easeBackFromRightEdge = true;
        }),
        OnOut = (Action<float>) (f =>
        {
          lightning.Update();
          if (!easeBackFromRightEdge)
            return;
          X += 128f * Engine.RawDeltaTime;
        })
      });
      Add((Component) (prechargeSfx = new SoundSource()));
      Add((Component) (chargeSfx = new SoundSource()));
      //1 -> -1
      Distort.AnxietyOrigin = new Vector2(-1f, 0.5f);
      Sprite.Scale.X *= -1;
    }

    public AngryOshiroRight(EntityData data, Vector2 offset)
      : this(data.Position + offset)
    {
    }

    public override void Added(Scene scene)
    {
      base.Added(scene);
      level = SceneAs<Level>();
      if (level.Session.GetFlag("oshiroEnding") || !level.Session.GetFlag("oshiro_resort_roof") && level.Session.Level.Equals("roof00"))
        RemoveSelf();
      if (state.State != 3)
        state.State = 4;
       Y = TargetY;
       cameraXOffset = 48f;
    }

    private float TargetY
    {
      get
      {
        Player entity = level.Tracker.GetEntity<Player>();
        if (entity != null)
          return MathHelper.Clamp(entity.CenterY, (float) (level.Bounds.Top + 8), (float) (level.Bounds.Bottom - 8));
        return Y;
      }
    }

    private void OnPlayer(Player player)
    {
      if (state.State == 5 || (double) CenterX <= (double) player.CenterX - 4.0 && !(Sprite.CurrentAnimationID != "respawn"))
        return;
      player.Die((player.Center - Center).SafeNormalize(Vector2.UnitX), false, true);
    }

    private void OnPlayerBounce(Player player)
    {
      if (state.State != 2 || (double) player.Bottom > (double) Top + 6.0)
        return;
      Audio.Play("event:/game/general/thing_booped", Position);
      Celeste.Freeze(0.2f);
      player.Bounce(Top + 2f);
      state.State = 5;
      prechargeSfx.Stop(true);
      chargeSfx.Stop(true);
    }

    public override void Update()
    {
      base.Update();
      Sprite.Scale.X = Calc.Approach(Sprite.Scale.X, -1f, 0.6f * Engine.DeltaTime);
      Sprite.Scale.Y = Calc.Approach(Sprite.Scale.Y, 1f, 0.6f * Engine.DeltaTime);
      if (!doRespawnAnim)
        Visible = (double) X < (double) level.Bounds.Right + (double) Width / 2.0;
      yApproachSpeed = Calc.Approach(yApproachSpeed, 100f, 300f * Engine.DeltaTime);
      if (state.State != 3 && canControlTimeRate)
      {
        if (state.State == 2 && (double) attackSpeed > 200.0)
        {
          Player entity = Scene.Tracker.GetEntity<Player>();
          Engine.TimeRate = entity == null || entity.Dead || (double) CenterX <= (double) entity.CenterX - 4.0 ? 1f : MathHelper.Lerp(Calc.ClampedMap(-entity.CenterX + CenterX, 30f, 80f, 0.5f, 1f), 1f, Calc.ClampedMap(Math.Abs(entity.CenterY - CenterY), 32f, 48f, 0.0f, 1f));
        }
        else
          Engine.TimeRate = 1f;
        Distort.GameRate = Calc.Approach(Distort.GameRate, Calc.Map(Engine.TimeRate, 0.5f, 1f, 0.0f, 1f), Engine.DeltaTime * 8f);
        Distort.Anxiety = Calc.Approach(Distort.Anxiety, targetAnxiety, anxietySpeed * Engine.DeltaTime);
      }
      else
      {
        Distort.GameRate = 1f;
        Distort.Anxiety = 0.0f;
      }
    }

    public void StopControllingTime()
    {
      canControlTimeRate = false;
    }

    public override void Render()
    {
      if (lightningVisible)
      {
        lightning.RenderPosition = new Vector2(level.Camera.Left - 2f, Top + 16f);
        lightning.Render();
      }
      Sprite.Position = shaker.Value * 2f;
      base.Render();
    }

    public void Leave()
    {
      leaving = true;
    }

    public void Squish()
    {
      //-1.3?
      Sprite.Scale = new Vector2(-1.3f, 0.5f);
      shaker.ShakeFor(0.5f, false);
    }

    private void ChaseBegin()
    {
      Sprite.Play("idle", false, false);
    }

    private int ChaseUpdate()
    {
      if (!hasEnteredSfx && (double) cameraXOffset <= 16.0 && !doRespawnAnim)
      {
        Audio.Play("event:/char/oshiro/boss_enter_screen", Position);
        hasEnteredSfx = true;
      }
      if (doRespawnAnim && (double) cameraXOffset <= 0.0)
      {
        Collider.Position.X = 48f;
        Visible = true;
        Sprite.Play("respawn", false, false);
        doRespawnAnim = false;
        if (Scene.Tracker.GetEntity<Player>() != null)
          Audio.Play("event:/char/oshiro/boss_reform", Position);
      }
      cameraXOffset = Calc.Approach(cameraXOffset, -20f, 80f * Engine.DeltaTime);
      X = level.Camera.Right + cameraXOffset;
      Collider.Position.X = Calc.Approach(Collider.Position.X, colliderTargetPosition.X, Engine.DeltaTime * 128f);
      Collidable = Visible;
      if (level.Tracker.GetEntity<Player>() != null && Sprite.CurrentAnimationID != "respawn")
        CenterY = Calc.Approach(CenterY, TargetY, yApproachSpeed * Engine.DeltaTime);
      return 0;
    }

    private IEnumerator ChaseCoroutine()
    {
      if ((uint) level.Session.Area.Mode > 0U)
      {
        yield return (object) 1f;
      }
      else
      {
        yield return (object) ChaseWaitTimes[attackIndex];
        ++attackIndex;
        attackIndex %= ChaseWaitTimes.Length;
      }
      prechargeSfx.Play("event:/char/oshiro/boss_precharge", (string) null, 0.0f);
      Sprite.Play("charge", false, false);
      yield return (object) 0.7f;
      if (Scene.Tracker.GetEntity<Player>() != null)
      {
        Alarm.Set((Entity) this, 0.216f, (Action) (() => chargeSfx.Play("event:/char/oshiro/boss_charge", (string) null, 0.0f)), Alarm.AlarmMode.Oneshot);
        state.State = 1;
      }
      else
        Sprite.Play("idle", false, false);
    }

    private int ChargeUpUpdate()
    {
      if (level.OnInterval(0.05f))
        Sprite.Position = Calc.Random.ShakeVector();
      cameraXOffset = Calc.Approach(cameraXOffset, 0.0f, 40f * Engine.DeltaTime);
      X = level.Camera.Right + cameraXOffset;
      Player entity = level.Tracker.GetEntity<Player>();
      if (entity != null)
        CenterY = Calc.Approach(CenterY, MathHelper.Clamp(entity.CenterY, (float) (level.Bounds.Top + 8), (float) (level.Bounds.Bottom - 8)), 30f * Engine.DeltaTime);
      return 1;
    }

    private void ChargeUpEnd()
    {
      Sprite.Position = Vector2.Zero;
    }

    private IEnumerator ChargeUpCoroutine()
    {
      Celeste.Freeze(0.05f);
      Distort.Anxiety = 0.3f;
      Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
      lightningVisible = true;
      lightning.Play("once", true, false);
      yield return (object) 0.3f;
      Player player = Scene.Tracker.GetEntity<Player>();
      state.State = player == null ? 0 : 2;
    }

    private void AttackBegin()
    {
      attackSpeed = 0.0f;
      targetAnxiety = 0.3f;
      anxietySpeed = 4f;
      level.DirectionalShake(Vector2.UnitX, 0.3f);
    }

    private void AttackEnd()
    {
      targetAnxiety = 0.0f;
      anxietySpeed = 0.5f;
    }

    private int AttackUpdate()
    {
      X -= attackSpeed * Engine.DeltaTime;
      attackSpeed = Calc.Approach(attackSpeed, 500f, 2000f * Engine.DeltaTime);
      if ((double) X <= (double) level.Camera.Left - 48.0)
      {
        if (leaving)
        {
          RemoveSelf();
          return 2;
        }
        X = level.Camera.Right + 48f;
        cameraXOffset = 48f;
        doRespawnAnim = true;
        Visible = false;
        return 0;
      }
      Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
      if (Scene.OnInterval(0.05f))
        TrailManager.Add((Entity) this, Color.Red * 0.6f, 0.5f);
      return 2;
    }

    private IEnumerator AttackCoroutine()
    {
      yield return (object) 0.1f;
      targetAnxiety = 0.0f;
      anxietySpeed = 0.5f;
    }

    public bool DummyMode
    {
      get
      {
        return state.State == 3;
      }
    }

    public void EnterDummyMode()
    {
      state.State = 3;
    }

    public void LeaveDummyMode()
    {
      state.State = 0;
    }

    private int WaitingUpdate()
    {
      Player entity = Scene.Tracker.GetEntity<Player>();
      return entity != null && entity.Speed != Vector2.Zero && (double) entity.X < (double) (level.Bounds.Right - 48) ? 0 : 4;
    }

    private void HurtBegin()
    {
      Sprite.Play("hurt", true, false);
    }

    private int HurtUpdate()
    {
      X -= 100f * Engine.DeltaTime;
      Y += 200f * Engine.DeltaTime;
      if ((double) Top <= (double) (level.Bounds.Bottom + 20))
        return 5;
      if (leaving)
      {
        RemoveSelf();
        return 5;
      }
      X = level.Camera.Right + 48f;
      cameraXOffset = 48f;
      doRespawnAnim = true;
      Visible = false;
      return 0;
    }
  }
}
