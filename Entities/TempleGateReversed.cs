using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DJMapHelper.Entities {
    public class TempleGateReversed : Solid {
        private enum Types
        {
            CloseBehindPlayer,
            CloseBehindPlayerAlways,
            HoldingTheo,
            CloseBehindPlayerAndTheo,
        }
        
        private const int OpenHeight = 0;
        private const float HoldingWaitTime = 0.2f;
        private const float HoldingOpenDistSq = 4096f;
        private const float HoldingCloseDistSq = 6400f;
        private const int MinDrawHeight = 4;
        private readonly int closedHeight;
        private readonly Vector2 holdingCheckFrom;
        private readonly Shaker shaker;
        private readonly Sprite sprite;
        private readonly bool theoGate;
        private float drawHeight;
        private float drawHeightMoveSpeed;
        private float holdingWaitTimer = 0.2f;
        private bool lockState;
        private bool open;
        private readonly Types type;

        private TempleGateReversed(
            Vector2 position,
            int height,
            Types type,
            string spriteName)
            : base(position, 8f, height, true) {
            this.type = type;
            closedHeight = height;
            Add(sprite = GFX.SpriteBank.Create("TempleGate_" + spriteName));
            sprite.X = Collider.Width / 2f;
            sprite.Play("idle");
            Add(shaker = new Shaker(false));
            Depth = -9000;
            theoGate = spriteName.Equals("theo", StringComparison.InvariantCultureIgnoreCase);
            holdingCheckFrom = Position + new Vector2(Width / 2f, height / 2f);
        }

        public TempleGateReversed(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Height, data.Enum("type", Types.CloseBehindPlayer),
                data.Attr(nameof(sprite), "default")) { }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            if (type == Types.CloseBehindPlayer) {
                Player player = Scene.Tracker.GetEntity<Player>();
                if (player != null && player.Right > Left && player.Bottom >= Top &&
                    player.Top <= Bottom) {
                    StartOpen();
                    Add(new Coroutine(CloseBehindPlayer()));
                }
            }
            else if (type == Types.CloseBehindPlayerAlways) {
                StartOpen();
                Add(new Coroutine(CloseBehindPlayer()));
            }
            else if (type == Types.CloseBehindPlayerAndTheo) {
                StartOpen();
                Add(new Coroutine(CloseBehindPlayerAndTheo()));
            }
            else if (type == Types.HoldingTheo) {
                if (TheoIsNearby()) {
                    StartOpen();
                }
            }

            drawHeight = Math.Max(4f, Height);
        }

        private void Open() {
            Audio.Play(
                theoGate
                    ? "event:/game/05_mirror_temple/gate_theo_open"
                    : "event:/game/05_mirror_temple/gate_main_open", Position);
            holdingWaitTimer = HoldingWaitTime;
            drawHeightMoveSpeed = 200f;
            drawHeight = Height;
            shaker.ShakeFor(HoldingWaitTime, false);
            SetHeight(OpenHeight);
            sprite.Play("open", false, false);
            open = true;
        }

        private void StartOpen() {
            SetHeight(OpenHeight);
            drawHeight = 4f;
            open = true;
        }

        private void Close() {
            Audio.Play(
                theoGate
                    ? "event:/game/05_mirror_temple/gate_theo_close"
                    : "event:/game/05_mirror_temple/gate_main_close", Position);
            holdingWaitTimer = HoldingWaitTime;
            drawHeightMoveSpeed = 300f;
            drawHeight = Math.Max(4f, Height);
            shaker.ShakeFor(HoldingWaitTime, false);
            SetHeight(closedHeight);
            sprite.Play("hit", false, false);
            open = false;
        }

        private IEnumerator CloseBehindPlayer() {
            TempleGateReversed templeGateReversed = this;
            while (true) {
                Player player = templeGateReversed.Scene.Tracker.GetEntity<Player>();
                if (templeGateReversed.lockState || player == null ||
                    player.Right >= templeGateReversed.Left - MinDrawHeight) {
                    yield return null;
                }
                else {
                    break;
                }
            }

            templeGateReversed.Close();
        }

        private IEnumerator CloseBehindPlayerAndTheo() {
            TempleGateReversed templeGateReversed = this;
            while (true) {
                Player player = templeGateReversed.Scene.Tracker.GetEntity<Player>();
                if (player != null && player.Right < templeGateReversed.Left - MinDrawHeight) {
                    TheoCrystal theoCrystal = templeGateReversed.Scene.Tracker.GetEntity<TheoCrystal>();
                    if (!templeGateReversed.lockState && theoCrystal != null &&
                        theoCrystal.Right < templeGateReversed.Left - MinDrawHeight) {
                        break;
                    }
                }

                yield return null;
            }

            templeGateReversed.Close();
        }

        private bool TheoIsNearby() {
            TheoCrystal theoCrystal = Scene.Tracker.GetEntity<TheoCrystal>();
            if (theoCrystal != null && theoCrystal.X >= X - 10.0) {
                return Vector2.DistanceSquared(holdingCheckFrom, theoCrystal.Center) <
                       (open ? HoldingCloseDistSq : HoldingOpenDistSq);
            }

            return true;
        }

        private void SetHeight(int height) {
            if (height < Collider.Height) {
                Collider.Height = height;
            }
            else {
                float y = Y;
                int height1 = (int) Collider.Height;
                if (Collider.Height < 64.0) {
                    Y -= 64f - Collider.Height;
                    Collider.Height = 64f;
                }

                MoveVExact(height - height1);
                Y = y;
                Collider.Height = height;
            }
        }

        public override void Update() {
            base.Update();
            if (type == Types.HoldingTheo) {
                if (holdingWaitTimer > 0.0) {
                    holdingWaitTimer -= Engine.DeltaTime;
                }
                else if (!lockState) {
                    if (open && !TheoIsNearby()) {
                        Close();
                        CollideFirst<Player>(Position + new Vector2(8f, 0.0f))?.Die(Vector2.Zero, false, true);
                    }
                    else if (!open && TheoIsNearby()) {
                        Open();
                    }
                }
            }

            float target = Math.Max(4f, Height);
            if (drawHeight != target) {
                lockState = true;
                drawHeight = Calc.Approach(drawHeight, target, drawHeightMoveSpeed * Engine.DeltaTime);
            }
            else {
                lockState = false;
            }
        }

        public override void Render() {
            Vector2 vector2 = new Vector2(Math.Sign(shaker.Value.X), 0.0f);
            Draw.Rect(X - 2f, Y - 8f, 14f, 10f, Color.Black);
            sprite.DrawSubrect(Vector2.Zero + vector2,
                new Rectangle(0, (int) (sprite.Height - drawHeight), (int) sprite.Width, (int) drawHeight));
        }
    }
}