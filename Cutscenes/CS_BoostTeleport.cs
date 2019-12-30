using System;
using System.Collections;
using Celeste.Mod.DJMapHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using FMOD.Studio;
namespace Celeste.Mod.DJMapHelper.Cutscenes
{
    public class CS_BoostTeleport: CutsceneEntity
    {
        private Player player;
        private BadelineBoostTeleport boost;
        private BirdNPC bird;
        private float fadeToWhite;
        private Vector2 birdScreenPosition;
        private AscendManager.Streaks streaks;
        private Vector2 cameraWaveOffset;
        private Vector2 cameraOffset;
        private float timer;
        private Coroutine wave;
        private string normalRoom;
        private string normalColorGrade;
        private string keyRoom;
        private string keyColorGrade;
        private string goldenRoom;
        private string goldenColorGrade;
        private bool hasGolden;
        private bool hasKey;
        private bool keyFirst;
        
        public CS_BoostTeleport(Player player, BadelineBoostTeleport boost, string normalRoom, string normalColorGrade, string keyRoom, string keyColorGrade, string goldenRoom, string goldenColorGrade, bool keyFirst)
          : base(false, false)
        {
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

        public override void OnBegin(Level level)
        {
            Audio.SetMusic((string) null, true, true);
            ScreenWipe.WipeColor = Color.White;
            foreach (Component follower in player.Leader.Followers)
            {
                Strawberry entity = follower.Entity as Strawberry;
                if (entity != null && entity.Golden)
                {
                    hasGolden = true;
                    break;
                }
            }
            foreach (Component follower in player.Leader.Followers)
            {
                Key entity = follower.Entity as Key;
                if (entity != null)
                {
                    hasKey = true;
                    break;
                }
            }
            Add((Component) new Coroutine(Cutscene(), true));
        }

        private IEnumerator Cutscene()
        {
            Engine.TimeRate = 1f;
            boost.Active = false;
            yield return (object) null;
            yield return (object) 0.152f;
            cameraOffset = new Vector2(0.0f, -20f);
            boost.Active = true;
            player.EnforceLevelBounds = false;
            yield return (object) null;
            BlackholeBG blackhole = Level.Background.Get<BlackholeBG>();
            if (blackhole != null)
            {
                blackhole.Direction = -2.5f;
                blackhole.SnapStrength(Level, BlackholeBG.Strengths.High);
                blackhole.CenterOffset.Y = 100f;
                blackhole.OffsetOffset.Y = -50f;
                blackhole = (BlackholeBG) null;
            }
            Add((Component) (wave = new Coroutine(WaveCamera(), true)));
            Add((Component) new Coroutine(BirdRoutine(0.8f), true));
            Level.Add((Entity) (streaks = new AscendManager.Streaks((AscendManager) null)));
            for (float p = 0.0f; (double) p < 1.0; p += Engine.DeltaTime / 12f)
            {
                fadeToWhite = p;
                streaks.Alpha = p;
                foreach (Parallax parallax in Level.Foreground.GetEach<Parallax>("blackhole"))
                {
                    Parallax fg = parallax;
                    fg.FadeAlphaMultiplier = 1f - p;
                    fg = (Parallax) null;
                }
                yield return (object) null;
            }
            while (bird != null)
                yield return (object) null;
            FadeWipe wipe = new FadeWipe((Scene)Level, false, (Action) null);
            wipe.Duration = 4f;
            ScreenWipe.WipeColor = Color.White;
            
            
            float from = cameraOffset.Y;
            int to = 180;
            for (float p = 0.0f; (double) p < 1.0; p += Engine.DeltaTime / 2f)
            {
                cameraOffset.Y = from + ((float) to - from) * Ease.BigBackIn(p);
                yield return (object) null;
            }
            while ((double) wipe.Percent < 1.0)
                yield return (object) null;
            EndCutscene(Level, true);
        }

        public override void OnEnd(Level level)
        {
            if (WasSkipped && (boost != null && (HandleBase) boost.Ch9FinalBoostSfx != (HandleBase) null))
            {
                int num1 = (int) boost.Ch9FinalBoostSfx.stop(STOP_MODE.ALLOWFADEOUT);
                int num2 = (int) boost.Ch9FinalBoostSfx.release();
            }
            string nextLevelName = normalRoom;
            string nextColorGrade = normalColorGrade;

            if (hasKey && !hasGolden)
            {
                nextLevelName = keyRoom;
                nextColorGrade = keyColorGrade;
            }
            else if (!hasKey && hasGolden)
            {
                nextLevelName = goldenRoom;
                nextColorGrade = goldenColorGrade;
            }
            else if(hasKey && hasGolden && keyFirst)
            {
                nextLevelName = keyRoom;
                nextColorGrade = keyColorGrade;
            }
            else if(hasKey && hasGolden &&!keyFirst)
            {
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
            Level.OnEndOfFrame += (Action) (() =>
            {
                Level.TeleportTo(player, nextLevelName, nextLevelIntro, new Vector2?());
                if (Level.Wipe != null)
                    Level.Wipe.Cancel();
                if (nextColorGrade != "")
                {
                    if (nextColorGrade == "none")
                        nextColorGrade = null;
                    level.SnapColorGrade(nextColorGrade);
                }
                new FadeWipe((Scene) level, true, (Action) null).Duration = 2f;
                ScreenWipe.WipeColor = Color.White;
            });
        }

        private IEnumerator WaveCamera()
        {
            float timer = 0.0f;
            while (true)
            {
                cameraWaveOffset.X = (float) Math.Sin((double) timer) * 16f;
                cameraWaveOffset.Y = (float) (Math.Sin((double) timer * 0.5) * 16.0 + Math.Sin((double) timer * 0.25) * 8.0);
                timer += Engine.DeltaTime * 2f;
                yield return (object) null;
            }
        }

        private IEnumerator BirdRoutine(float delay)
        {
            yield return (object) delay;
            Level.Add((Entity) (bird = new BirdNPC(Vector2.Zero, BirdNPC.Modes.None)));
            bird.Sprite.Play("flyupIdle", false, false);
            Vector2 center = new Vector2(320f, 180f) / 2f;
            Vector2 topCenter = new Vector2(center.X, 0.0f);
            Vector2 botCenter = new Vector2(center.X, 180f);
            Vector2 from1 = botCenter + new Vector2(40f, 40f);
            Vector2 to1 = center + new Vector2(-32f, -24f);
            for (float t = 0.0f; (double) t < 1.0; t += Engine.DeltaTime / 4f)
            {
                birdScreenPosition = from1 + (to1 - from1) * Ease.BackOut(t);
                yield return (object) null;
            }
            bird.Sprite.Play("flyupRoll", false, false);
            for (float t = 0.0f; (double) t < 1.0; t += Engine.DeltaTime / 2f)
            {
                birdScreenPosition = to1 + new Vector2(64f, 0.0f) * Ease.CubeInOut(t);
                yield return (object) null;
            }
            from1 = new Vector2();
            to1 = new Vector2();
            Vector2 from2 = birdScreenPosition;
            Vector2 to2 = topCenter + new Vector2(-40f, -100f);
            bool playedAnim = false;
            for (float t = 0.0f; (double) t < 1.0; t += Engine.DeltaTime / 4f)
            {
                if ((double) t >= 0.349999994039536 && !playedAnim)
                {
                    bird.Sprite.Play("flyupRoll", false, false);
                    playedAnim = true;
                }
                birdScreenPosition = from2 + (to2 - from2) * Ease.BigBackIn(t);
                birdScreenPosition.X += t * 32f;
                yield return (object) null;
            }
            bird.RemoveSelf();
            bird = (BirdNPC) null;
            from2 = new Vector2();
            to2 = new Vector2();
        }

        public override void Update()
        {
            base.Update();
            timer += Engine.DeltaTime;
            if (bird != null)
            {
                bird.Position = Level.Camera.Position + birdScreenPosition;
                bird.Position.X += (float) Math.Sin((double) timer) * 4f;
                bird.Position.Y += (float) (Math.Sin((double) timer * 0.100000001490116) * 4.0 + Math.Sin((double) timer * 0.25) * 4.0);
            }
            Level.CameraOffset = cameraOffset + cameraWaveOffset;
        }

        public override void Render()
        {
            Camera camera = Level.Camera;
            Draw.Rect(camera.X - 1f, camera.Y - 1f, 322f, 322f, Color.White * fadeToWhite);
        }
    }
}