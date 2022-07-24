using System;
using System.Collections;
using Celeste.Mod.DJMapHelper.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DJMapHelper.Cutscenes; 

public class CS_BoostTeleport : CutsceneEntity {
    private readonly BadelineBoostTeleport boost;
    private readonly Player player;
    private BirdNPC bird;
    private Vector2 birdScreenPosition;
    private Vector2 cameraOffset;
    private Vector2 cameraWaveOffset;
    private float fadeToWhite;
    private AscendManager.Streaks streaks;
    private float timer;
    private Color screenWipeColor;
    private readonly BadelineBoostTeleport.Info info;

    public static void OnLoad() {
        On.Celeste.Player.IntroJumpCoroutine += PlayerOnIntroJumpCoroutine;
    }

    public static void OnUnload() {
        On.Celeste.Player.IntroJumpCoroutine -= PlayerOnIntroJumpCoroutine;
    }

    private static IEnumerator PlayerOnIntroJumpCoroutine(On.Celeste.Player.orig_IntroJumpCoroutine orig, Player player) {
        bool showPlayer = false;
        if (player.Get<ShowPlayerComponent>() is { } component) {
            player.Remove(component);
            showPlayer = true;
        }
        IEnumerator enumerator = orig(player);
        while (enumerator.MoveNext()) {
            yield return enumerator.Current;
            if (showPlayer) {
                player.Visible = true;
            }
        }
    }

    public CS_BoostTeleport(Player player, BadelineBoostTeleport boost, BadelineBoostTeleport.Info info) {
        this.player = player;
        this.boost = boost;
        this.info = info;
        Depth = 10010;
    }

    public override void OnBegin(Level level) {
        Audio.SetMusic(null);
        screenWipeColor = ScreenWipe.WipeColor;
        ScreenWipe.WipeColor = Color.White;

        Add(new Coroutine(Cutscene()));
    }

    private IEnumerator Cutscene() {
        Engine.TimeRate = 1f;
        boost.Active = false;
        yield return null;
        yield return 0.152f;
        cameraOffset = new Vector2(0.0f, -20f);
        boost.Active = true;
        player.EnforceLevelBounds = false;
        yield return null;
        BlackholeBG blackhole = Level.Background.Get<BlackholeBG>();
        if (blackhole != null) {
            blackhole.Direction = -2.5f;
            blackhole.SnapStrength(Level, BlackholeBG.Strengths.High);
            blackhole.CenterOffset.Y = 100f;
            blackhole.OffsetOffset.Y = -50f;
            blackhole = null;
        }

        Add(new Coroutine(WaveCamera()));
        Add(new Coroutine(BirdRoutine(0.8f)));
        Level.Add(streaks = new AscendManager.Streaks(null));
        for (var p = 0.0f; p < 1.0; p += Engine.DeltaTime / 12f) {
            fadeToWhite = p;
            streaks.Alpha = p;
            foreach (Parallax parallax in Level.Foreground.GetEach<Parallax>("blackhole")) {
                Parallax fg = parallax;
                fg.FadeAlphaMultiplier = 1f - p;
            }

            yield return null;
        }

        while (bird != null) {
            yield return null;
        }

        FadeWipe wipe = new FadeWipe(Level, false) {Duration = 4f};

        var from = cameraOffset.Y;
        var to = 180;
        for (var p = 0.0f; p < 1.0; p += Engine.DeltaTime / 2f) {
            cameraOffset.Y = from + (to - from) * Ease.BigBackIn(p);
            yield return null;
        }

        while (wipe.Percent < 1.0) {
            yield return null;
        }

        EndCutscene(Level);
    }

    public override void OnEnd(Level level) {
        ScreenWipe.WipeColor = screenWipeColor;

        if (WasSkipped && boost != null && boost.Ch9FinalBoostSfx != null) {
            boost.Ch9FinalBoostSfx.stop(STOP_MODE.ALLOWFADEOUT);
            boost.Ch9FinalBoostSfx.release();
        }

        player.Active = true;
        player.Speed = Vector2.Zero;
        player.EnforceLevelBounds = true;
        player.StateMachine.State = 0;
        player.DummyFriction = true;
        player.DummyGravity = true;
        player.DummyAutoAnimate = true;
        player.ForceCameraUpdate = false;
        Engine.TimeRate = 1f;
        Level.OnEndOfFrame += () => {
            Level.TeleportTo(player, info.Room, Player.IntroTypes.Jump, info.SpawnPoint);
            if (Level.Tracker.GetEntity<Player>() is { } newPlayer) {
                newPlayer.Visible = !WasSkipped;
                if (WasSkipped) {
                    newPlayer.Add(new ShowPlayerComponent());
                }
            }

            if (info.ColorGrade != "") {
                if (info.ColorGrade == "none") {
                    info.ColorGrade = null;
                }

                level.SnapColorGrade(info.ColorGrade);
            }
        };
    }

    public override void Removed(Scene scene) {
        ScreenWipe.WipeColor = screenWipeColor;
    }

    private IEnumerator WaveCamera() {
        float waveTimer = 0.0f;
        while (true) {
            cameraWaveOffset.X = (float) Math.Sin(waveTimer) * 16f;
            cameraWaveOffset.Y = (float) (Math.Sin(waveTimer * 0.5) * 16.0 + Math.Sin(waveTimer * 0.25) * 8.0);
            waveTimer += Engine.DeltaTime * 2f;
            yield return null;
        }
    }

    private IEnumerator BirdRoutine(float delay) {
        yield return delay;
        Level.Add(bird = new BirdNPC(Vector2.Zero, BirdNPC.Modes.None));
        bird.Sprite.Play("flyupIdle");
        Vector2 center = new Vector2(320f, 180f) / 2f;
        Vector2 topCenter = new Vector2(center.X, 0.0f);
        Vector2 botCenter = new Vector2(center.X, 180f);
        Vector2 from1 = botCenter + new Vector2(40f, 40f);
        Vector2 to1 = center + new Vector2(-32f, -24f);
        for (var t = 0.0f; t < 1.0; t += Engine.DeltaTime / 4f) {
            birdScreenPosition = from1 + (to1 - from1) * Ease.BackOut(t);
            yield return null;
        }

        bird.Sprite.Play("flyupRoll");
        for (var t = 0.0f; t < 1.0; t += Engine.DeltaTime / 2f) {
            birdScreenPosition = to1 + new Vector2(64f, 0.0f) * Ease.CubeInOut(t);
            yield return null;
        }

        Vector2 from2 = birdScreenPosition;
        Vector2 to2 = topCenter + new Vector2(-40f, -100f);
        var playedAnim = false;
        for (var t = 0.0f; t < 1.0; t += Engine.DeltaTime / 4f) {
            if (t >= 0.349999994039536 && !playedAnim) {
                bird.Sprite.Play("flyupRoll");
                playedAnim = true;
            }

            birdScreenPosition = from2 + (to2 - from2) * Ease.BigBackIn(t);
            birdScreenPosition.X += t * 32f;
            yield return null;
        }

        bird.RemoveSelf();
        bird = null;
    }

    public override void Update() {
        base.Update();
        timer += Engine.DeltaTime;
        if (bird != null) {
            bird.Position = Level.Camera.Position + birdScreenPosition;
            bird.Position.X += (float) Math.Sin(timer) * 4f;
            bird.Position.Y += (float) (Math.Sin(timer * 0.100000001490116) * 4.0 + Math.Sin(timer * 0.25) * 4.0);
        }

        Level.CameraOffset = cameraOffset + cameraWaveOffset;
    }

    public override void Render() {
        Camera camera = Level.Camera;
        Draw.Rect(camera.X - 1f, camera.Y - 1f, 322f, 322f, Color.White * fadeToWhite);
    }
}

public class ShowPlayerComponent : Component {
    public ShowPlayerComponent() : base(false, false) { }
}