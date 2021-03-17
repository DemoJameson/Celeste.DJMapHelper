using System;
using System.Collections;
using System.Linq;
using Celeste.Mod.DJMapHelper.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DJMapHelper.Cutscenes {
    public class CS_BoostTeleport : CutsceneEntity {
        private readonly BadelineBoostTeleport boost;
        private readonly string goldenColorGrade;
        private readonly string goldenRoom;
        private readonly string keyColorGrade;
        private readonly bool keyFirst;
        private readonly string keyRoom;
        private readonly string normalColorGrade;
        private readonly string normalRoom;
        private readonly Player player;
        private BirdNPC bird;
        private Vector2 birdScreenPosition;
        private Vector2 cameraOffset;
        private Vector2 cameraWaveOffset;
        private float fadeToWhite;
        private bool hasGolden;
        private bool hasKey;
        private AscendManager.Streaks streaks;
        private float timer;
        private Color screenWipeColor;

        public CS_BoostTeleport(Player player, BadelineBoostTeleport boost, string normalRoom, string normalColorGrade,
            string keyRoom, string keyColorGrade, string goldenRoom, string goldenColorGrade, bool keyFirst) {
            this.player = player;
            this.boost = boost;
            this.normalRoom = normalRoom;
            this.normalColorGrade = normalColorGrade;
            this.keyRoom = keyRoom;
            this.keyColorGrade = keyColorGrade;
            this.goldenRoom = goldenRoom;
            this.goldenColorGrade = goldenColorGrade;
            this.keyFirst = keyFirst;
            hasKey = false;
            hasGolden = false;
            Depth = 10010;
        }

        public override void OnBegin(Level level) {
            Audio.SetMusic(null);
            screenWipeColor = ScreenWipe.WipeColor;
            ScreenWipe.WipeColor = Color.White;

            hasGolden = player.Leader.Followers.Any(follower => follower.Entity is Strawberry berry && berry.Golden);
            hasKey = player.Leader.Followers.Any(follower => follower.Entity is Key);

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
            for (var p = 0.0f; (double) p < 1.0; p += Engine.DeltaTime / 12f) {
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
            for (var p = 0.0f; (double) p < 1.0; p += Engine.DeltaTime / 2f) {
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

            var nextLevelName = normalRoom;
            var nextColorGrade = normalColorGrade;

            if (hasKey && !hasGolden && keyRoom != "") {
                nextLevelName = keyRoom;
                nextColorGrade = keyColorGrade;
            }
            else if (!hasKey && hasGolden && goldenRoom != "") {
                nextLevelName = goldenRoom;
                nextColorGrade = goldenColorGrade;
            }
            else if (hasKey && hasGolden && keyFirst && keyRoom != "") {
                nextLevelName = keyRoom;
                nextColorGrade = keyColorGrade;
            }
            else if (hasKey && hasGolden && !keyFirst && goldenRoom != "") {
                nextLevelName = goldenRoom;
                nextColorGrade = goldenColorGrade;
            }

            Player.IntroTypes nextLevelIntro = Player.IntroTypes.Jump;
            player.Active = true;
            player.Speed = Vector2.Zero;
            player.EnforceLevelBounds = true;
            player.StateMachine.State = 0;
            player.DummyFriction = true;
            player.DummyGravity = true;
            player.DummyAutoAnimate = true;
            player.ForceCameraUpdate = false;
            Engine.TimeRate = 1f;
            Level.OnEndOfFrame += (Action) (() => {
                Level.TeleportTo(player, nextLevelName, nextLevelIntro);
                if (nextColorGrade != "") {
                    if (nextColorGrade == "none") {
                        nextColorGrade = null;
                    }

                    level.SnapColorGrade(nextColorGrade);
                }
            });
        }

        private IEnumerator WaveCamera() {
            var timer = 0.0f;
            while (true) {
                cameraWaveOffset.X = (float) Math.Sin(timer) * 16f;
                cameraWaveOffset.Y = (float) (Math.Sin(timer * 0.5) * 16.0 + Math.Sin(timer * 0.25) * 8.0);
                timer += Engine.DeltaTime * 2f;
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
            for (var t = 0.0f; (double) t < 1.0; t += Engine.DeltaTime / 4f) {
                birdScreenPosition = from1 + (to1 - from1) * Ease.BackOut(t);
                yield return null;
            }

            bird.Sprite.Play("flyupRoll");
            for (var t = 0.0f; (double) t < 1.0; t += Engine.DeltaTime / 2f) {
                birdScreenPosition = to1 + new Vector2(64f, 0.0f) * Ease.CubeInOut(t);
                yield return null;
            }

            from1 = new Vector2();
            to1 = new Vector2();
            Vector2 from2 = birdScreenPosition;
            Vector2 to2 = topCenter + new Vector2(-40f, -100f);
            var playedAnim = false;
            for (var t = 0.0f; (double) t < 1.0; t += Engine.DeltaTime / 4f) {
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
}