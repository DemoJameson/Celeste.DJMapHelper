using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Celeste.Mod.DJMapHelper;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste {
    public class FlingBirdReversed : FlingBird {
        private static readonly FieldInfo EntityDataFieldInfo = typeof(FlingBird).GetPrivateField("entityData");
        private static readonly FieldInfo FlingBirdFieldInfo = typeof(Player).GetPrivateField("flingBird");
        private static readonly FieldInfo ForceMoveXFieldInfo = typeof(Player).GetPrivateField("forceMoveX");
        private static readonly IntPtr UpdatePtr = typeof(Entity).GetMethod("Update").MethodHandle.GetFunctionPointer();
        private static readonly IntPtr AwakePtr = typeof(Entity).GetMethod("Awake").MethodHandle.GetFunctionPointer();
        
        public static void OnLoad() {
           On.Celeste.FlingBird.Awake += FlingBirdOnAwake; 
           On.Celeste.Player.FinishFlingBird += PlayerOnFinishFlingBird;
        }
        
        public static void OnUnLoad() {
            On.Celeste.FlingBird.Awake -= FlingBirdOnAwake; 
            On.Celeste.Player.FinishFlingBird -= PlayerOnFinishFlingBird;
        }

        private static void PlayerOnFinishFlingBird(On.Celeste.Player.orig_FinishFlingBird orig, Player self) {
            orig(self);
            FlingBird flingBird = (FlingBird) FlingBirdFieldInfo.GetValue(self);
            if (flingBird is FlingBirdReversed) {
                self.Speed *= -1;
                ForceMoveXFieldInfo.SetValue(self, -1);
            }
        }

        // 重写过滤掉 FlingBirdReversed;
        private static void FlingBirdOnAwake(On.Celeste.FlingBird.orig_Awake orig, FlingBird self, Scene scene) {
            var baseAwake = (Action<Scene>) Activator.CreateInstance(typeof(Action<Scene>), self, AwakePtr);
            baseAwake(scene);
            
            List<FlingBird> all = scene.Entities.FindAll<FlingBird>().Where((item)=>!(item is FlingBirdReversed)).ToList();
            EntityData entityData = (EntityData) EntityDataFieldInfo.GetValue(self);
            for (int index = all.Count - 1; index >= 0; --index) {
                if (((EntityData)EntityDataFieldInfo.GetValue(all[index])).Level.Name != entityData.Level.Name) {
                    all.RemoveAt(index);
                }
            }

            all.Sort((a, b) => Math.Sign(a.X - b.X));
            if (all[0] == self) {
                for (int index = 1; index < all.Count; ++index) {
                    self.NodeSegments.Add(all[index].NodeSegments[0]);
                    self.SegmentsWaiting.Add(all[index].SegmentsWaiting[0]);
                    all[index].RemoveSelf();
                }
            }

            if (self.SegmentsWaiting[0]) {
                var sprite = self.Get<Sprite>();
                sprite.Play("hoverStressed");
                sprite.Scale.X = 1f;
            }

            Player player = scene.Tracker.GetEntity<Player>();
            if (player == null || player.X <= (double) self.X) {
                return;
            }

            self.RemoveSelf();
        }

        private Action _baseUpdate;
        private Action<Scene> _baseAwake;

        private void baseUpdate() {
            if (_baseUpdate == null) {
                _baseUpdate = (Action) Activator.CreateInstance(typeof(Action), this, UpdatePtr);
            }

            _baseUpdate();
        }

        private void baseAwake(Scene scene) {
            if (_baseAwake == null) {
                _baseAwake = (Action<Scene>) Activator.CreateInstance(typeof(Action<Scene>), this, AwakePtr);
            }

            _baseAwake(scene);
        }


        public new const float SkipDist = 100f;
        public new static readonly Vector2 FlingSpeed = new Vector2(380f, -100f);
        private readonly EntityData entityData;
        private float flingAccel;
        private Vector2 flingSpeed;
        private Vector2 flingTargetSpeed;
        private readonly SoundSource moveSfx;
        private new List<Vector2[]> NodeSegments;
        private int segmentIndex;
        private new List<bool> SegmentsWaiting;
        private readonly Sprite sprite;
        private readonly Vector2 spriteOffset = new Vector2(0f, 8f);
        private States state;
        private readonly Color trailColor = Calc.HexToColor("639bff");

        public FlingBirdReversed(Vector2[] nodes, bool skippAble) : base(nodes, skippAble) {
            Get<Sprite>()?.RemoveSelf();
            Get<PlayerCollider>()?.RemoveSelf();
            Get<SoundSource>()?.RemoveSelf();
            Get<TransitionListener>()?.RemoveSelf();

            Add(sprite = GFX.SpriteBank.Create("bird"));
            sprite.Play("hover");
            // sprite.Scale.X = -1f;
            sprite.Scale.X = 1f;
            sprite.Position = spriteOffset;
            sprite.OnFrameChange = delegate { BirdNPC.FlapSfxCheck(sprite); };
            Collider = new Circle(16f);
            Add(new PlayerCollider(OnPlayer));
            Add(moveSfx = new SoundSource());
            NodeSegments = new List<Vector2[]>();
            NodeSegments.Add(nodes);
            SegmentsWaiting = new List<bool>();
            SegmentsWaiting.Add(skippAble);
            Add(new TransitionListener {
                OnOut = delegate(float t) { sprite.Color = Color.White * (1f - Calc.Map(t, 0f, 0.4f)); }
            });
        }

        public FlingBirdReversed(EntityData data, Vector2 levelOffset) : this(data.NodesWithPosition(levelOffset), data.Bool("waiting")) {
            entityData = data;
        }

        public override void Awake(Scene scene) {
            baseAwake(scene);
            List<FlingBirdReversed> birds = Scene.Entities.FindAll<FlingBirdReversed>();
            for (int i = birds.Count - 1; i >= 0; i--) {
                if (birds[i].entityData.Level.Name != entityData.Level.Name) {
                    birds.RemoveAt(i);
                }
            }

            // birds.Sort((a, b) => Math.Sign(a.X - b.X));
            birds.Sort((a, b) => Math.Sign(b.X - a.X));
            if (birds[0] == this) {
                for (int j = 1; j < birds.Count; j++) {
                    NodeSegments.Add(birds[j].NodeSegments[0]);
                    SegmentsWaiting.Add(birds[j].SegmentsWaiting[0]);
                    birds[j].RemoveSelf();
                }
            }

            if (SegmentsWaiting[0]) {
                sprite.Play("hoverStressed");
                // sprite.Scale.X = 1f;
                sprite.Scale.X = -1f;
            }

            Player player = scene.Tracker.GetEntity<Player>();
            // if (player != null && player.X > X) {
            if (player != null && player.X < X) {
                RemoveSelf();
            }
        }

        private void Skip() {
            state = States.Move;
            Add(new Coroutine(MoveRoutine()));
        }

        private void OnPlayer(Player player) {
            if (state == States.Wait && player.DoFlingBird(this)) {
                flingSpeed = player.Speed * 0.4f;
                flingSpeed.Y = 120f;
                flingTargetSpeed = Vector2.Zero;
                flingAccel = 1000f;
                player.Speed = Vector2.Zero;
                state = States.Fling;
                Add(new Coroutine(DoFlingRoutine(player)));
                Audio.Play("event:/new_content/game/10_farewell/bird_throw", Center);
            }
        }

        public override void Update() {
            baseUpdate();
            if (state != States.Wait) {
                sprite.Position = Calc.Approach(sprite.Position, spriteOffset, 32f * Engine.DeltaTime);
            }

            switch (state) {
                case States.Wait: {
                    Player player = Scene.Tracker.GetEntity<Player>();
                    // if (player != null && player.X - X >= 100f) {
                    if (player != null && X - player.X >= 100f) {
                        Skip();
                        return;
                    }

                    if (SegmentsWaiting[segmentIndex] && LightningRemoved) {
                        Skip();
                        return;
                    }

                    if (player != null) {
                        float dist = Calc.ClampedMap((player.Center - Position).Length(), 16f, 64f, 12f, 0f);
                        Vector2 dir = (player.Center - Position).SafeNormalize();
                        sprite.Position = Calc.Approach(sprite.Position, spriteOffset + dir * dist, 32f * Engine.DeltaTime);
                    }

                    break;
                }
                case States.Fling:
                    if (flingAccel > 0f) {
                        flingSpeed = Calc.Approach(flingSpeed, flingTargetSpeed, flingAccel * Engine.DeltaTime);
                    }

                    // Position += flingSpeed * Engine.DeltaTime;
                    Position -= flingSpeed * Engine.DeltaTime;
                    return;
                case States.Move:
                    break;
                case States.WaitForLightningClear:
                    // if (Scene.Entities.FindFirst<Lightning>() == null || X > (Scene as Level).Bounds.Right) {
                    if (Scene.Entities.FindFirst<Lightning>() == null || X < (Scene as Level).Bounds.Left) {
                        // sprite.Scale.X = 1f;
                        sprite.Scale.X = -1f;
                        state = States.Leaving;
                        Add(new Coroutine(LeaveRoutine()));
                    }

                    break;
                default:
                    return;
            }
        }

        private IEnumerator DoFlingRoutine(Player player) {
            Level level = Scene as Level;
            Vector2 camera = level.Camera.Position;
            Vector2 zoom = player.Position - camera;
            zoom.X = Calc.Clamp(zoom.X, 145f, 215f);
            zoom.Y = Calc.Clamp(zoom.Y, 85f, 95f);
            Add(new Coroutine(level.ZoomTo(zoom, 1.1f, 0.2f)));
            Engine.TimeRate = 0.8f;
            Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
            while (flingSpeed != Vector2.Zero) {
                yield return null;
            }

            sprite.Play("throw");
            // sprite.Scale.X = 1f;
            sprite.Scale.X = -1f;
            flingSpeed = new Vector2(-140f, 140f);
            flingTargetSpeed = Vector2.Zero;
            flingAccel = 1400f;
            yield return 0.1f;
            Celeste.Freeze(0.05f);
            flingTargetSpeed = FlingSpeed;
            flingAccel = 6000f;
            yield return 0.1f;
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            Engine.TimeRate = 1f;
            level.Shake();
            Add(new Coroutine(level.ZoomBack(0.1f)));
            player.FinishFlingBird();
            flingTargetSpeed = Vector2.Zero;
            flingAccel = 4000f;
            yield return 0.3f;
            Add(new Coroutine(MoveRoutine()));
        }

        private IEnumerator MoveRoutine() {
            state = States.Move;
            sprite.Play("fly");
            // sprite.Scale.X = 1f;
            sprite.Scale.X = -1f;
            moveSfx.Play("event:/new_content/game/10_farewell/bird_relocate");
            for (int nodeIndex = 1; nodeIndex < NodeSegments[segmentIndex].Length - 1; nodeIndex += 2) {
                Vector2 from = Position;
                Vector2 anchor = NodeSegments[segmentIndex][nodeIndex];
                Vector2 to = NodeSegments[segmentIndex][nodeIndex + 1];
                yield return MoveOnCurve(from, anchor, to);
            }

            segmentIndex++;
            bool atEnding = segmentIndex >= NodeSegments.Count;
            if (!atEnding) {
                Vector2 from2 = Position;
                Vector2 anchor2 = NodeSegments[segmentIndex - 1][NodeSegments[segmentIndex - 1].Length - 1];
                Vector2 to2 = NodeSegments[segmentIndex][0];
                yield return MoveOnCurve(from2, anchor2, to2);
            }

            sprite.Rotation = 0f;
            sprite.Scale = Vector2.One;
            if (atEnding) {
                sprite.Play("hoverStressed");
                // sprite.Scale.X = 1f;
                sprite.Scale.X = -1f;
                state = States.WaitForLightningClear;
            }
            else {
                if (SegmentsWaiting[segmentIndex]) {
                    sprite.Play("hoverStressed");
                }
                else {
                    sprite.Play("hover");
                }

                // sprite.Scale.X = -1f;
                sprite.Scale.X = 1f;
                state = States.Wait;
            }
        }

        private IEnumerator LeaveRoutine() {
            // sprite.Scale.X = 1f;
            sprite.Scale.X = -1f;
            sprite.Play("fly");
            // Vector2 to = new Vector2((Scene as Level).Bounds.Right + 32, Y);
            Vector2 to = new Vector2((Scene as Level).Bounds.Left - 32, Y);
            yield return MoveOnCurve(Position, (Position + to) * 0.5f - Vector2.UnitY * 12f, to);
            RemoveSelf();
        }

        private IEnumerator MoveOnCurve(Vector2 from, Vector2 anchor, Vector2 to) {
            SimpleCurve curve = new SimpleCurve(from, to, anchor);
            float duration = curve.GetLengthParametric(32) / 500f;
            Vector2 was = from;
            for (float t = 0.016f; t <= 1f; t += Engine.DeltaTime / duration) {
                Position = curve.GetPoint(t).Floor();
                sprite.Rotation = Calc.Angle(curve.GetPoint(Math.Max(0f, t - 0.05f)), curve.GetPoint(Math.Min(1f, t + 0.05f)));
                // sprite.Scale.X = 1.25f;
                sprite.Scale.X = -1.25f;
                sprite.Scale.Y = 0.7f;
                if ((was - Position).Length() > 32f) {
                    TrailManager.Add(this, trailColor, 1f, false);
                    was = Position;
                }

                yield return null;
            }

            Position = to;
        }

        private enum States {
            Wait,
            Fling,
            Move,
            WaitForLightningClear,
            Leaving
        }
    }
}