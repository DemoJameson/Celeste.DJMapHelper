using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.DJMapHelper.Cutscenes;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using EventInstance = FMOD.Studio.EventInstance;

namespace Celeste.Mod.DJMapHelper.Entities; 

[CustomEntity("DJMapHelper/badelineBoostTeleport")]
public class BadelineBoostTeleport : Entity {
    public enum Collectable {
        None, Key, Golden, Moon
    }

    public record Info {
        public Collectable Collectable;
        public string Room;
        public string ColorGrade;
        public Vector2 SpawnPoint;
    }

    private const float MoveSpeed = 320f;
    private readonly BloomPoint bloom;
    private readonly VertexLight light;
    private readonly Vector2[] nodes;
    private readonly SoundSource relocateSfx;
    private readonly Sprite sprite;
    private readonly Image stretch;
    private readonly Wiggler wiggler;
    public EventInstance Ch9FinalBoostSfx;
    private Player holding;
    private int nodeIndex;
    private bool travelling;
    private readonly List<Info> infos = new();

    private BadelineBoostTeleport(Vector2[] nodes,
        string defaultRoom, string defaultColorGrade, int defaultSpawnPointX, int defaultSpawnPointY,
        string keyRoom, string keyColorGrade, int keySpawnPointX, int keySpawnPointY,
        string goldenRoom, string goldenColorGrade, int goldenSpawnPointX, int goldenSpawnPointY,
        string moonRoom, string moonColorGrade, int moonSpawnPointX, int moonSpawnPointY,
        string priority)
        : base(nodes[0]) {
        Depth = -1000000;
        this.nodes = nodes;

        if (!string.IsNullOrWhiteSpace(keyRoom)) {
            infos.Add(new Info {
                Collectable = Collectable.Key,
                Room = keyRoom,
                ColorGrade = keyColorGrade,
                SpawnPoint = new Vector2(keySpawnPointX, keySpawnPointY)
            });
        }

        if (!string.IsNullOrWhiteSpace(goldenRoom)) {
            infos.Add(new Info {
                Collectable = Collectable.Golden,
                Room = goldenRoom,
                ColorGrade = goldenColorGrade,
                SpawnPoint = new Vector2(goldenSpawnPointX, goldenSpawnPointY)
            });
        }

        if (!string.IsNullOrWhiteSpace(moonRoom)) {
            infos.Add(new Info {
                Collectable = Collectable.Moon,
                Room = moonRoom,
                ColorGrade = moonColorGrade,
                SpawnPoint = new Vector2(moonSpawnPointX, moonSpawnPointY)
            });
        }


        if (string.IsNullOrWhiteSpace(priority)) {
            priority = "Moon -> Golden -> Key";
        }

        List<Collectable> priorities = priority.Split(new[] { "->" }, StringSplitOptions.None).Select(s => (Collectable)Enum.GetNames(typeof(Collectable)).ToList().IndexOf(s.Trim())).ToList();
        infos.Sort((a, b) => priorities.IndexOf(a.Collectable) - priorities.IndexOf(b.Collectable));

        if (!string.IsNullOrWhiteSpace(defaultRoom)) {
            infos.Add(new Info {
                Collectable = Collectable.None,
                Room = defaultRoom,
                ColorGrade = defaultColorGrade,
                SpawnPoint = new Vector2(defaultSpawnPointX, defaultSpawnPointY)
            });
        }

        Collider = new Circle(16f);
        Add(new PlayerCollider(OnPlayer));
        Add(sprite = GFX.SpriteBank.Create("badelineBoost"));
        Add(stretch = new Image(GFX.Game["objects/badelineboost/stretch"]));
        stretch.Visible = false;
        stretch.CenterOrigin();
        Add(light = new VertexLight(Color.White, 0.7f, 12, 20));
        Add(bloom = new BloomPoint(0.5f, 12f));
        Add(wiggler = Wiggler.Create(0.4f, 3f,
            f => sprite.Scale = Vector2.One * (float) (1.0 + (double) wiggler.Value * 0.400000005960464)));
        Add(relocateSfx = new SoundSource());
    }

    public BadelineBoostTeleport(EntityData data, Vector2 offset)
        : this(data.NodesWithPosition(offset),
            data.Attr("Room", data.Attr("DefaultRoom")), data.Attr("ColorGrade", data.Attr("DefaultColorGrade")), data.Int("DefaultSpawnPointX"), data.Int("DefaultSpawnPointY"),
            data.Attr("KeyRoom"), data.Attr("KeyColorGrade"), data.Int("KeySpawnPointX"), data.Int("KeySpawnPointY"),
            data.Attr("GoldenRoom"), data.Attr("GoldenColorGrade"), data.Int("GoldenSpawnPointX"), data.Int("GoldenSpawnPointY"),
            data.Attr("MoonRoom"), data.Attr("MoonColorGrade"), data.Int("MoonSpawnPointX"), data.Int("MoonSpawnPointY"),
            data.Attr("Priority")) { }

    public override void Awake(Scene scene) {
        base.Awake(scene);
        if (!CollideCheck<FakeWall>()) {
            return;
        }

        Depth = -12500;
    }

    private void OnPlayer(Player player) {
        Add(new Coroutine(BoostRoutine(player)));
    }

    private IEnumerator BoostRoutine(Player player) {
        holding = player;
        travelling = true;
        ++nodeIndex;
        sprite.Visible = false;
        sprite.Position = Vector2.Zero;
        Collidable = false;
        var finalBoost = nodeIndex >= nodes.Length;
        Level level = player.SceneAs<Level>();
        if (finalBoost) {
            Audio.Play("event:/new_content/char/badeline/booster_finalfinal_part1", Position);
        } else {
            Audio.Play("event:/char/badeline/booster_begin", Position);
        }

        if (player.Holding != null) {
            player.Drop();
        }

        player.StateMachine.State = 11;
        player.DummyAutoAnimate = false;
        player.DummyGravity = false;
        if (player.Inventory.Dashes > 1) {
            player.Dashes = 1;
        } else {
            player.RefillDash();
        }

        player.RefillStamina();
        player.Speed = Vector2.Zero;
        var side = Math.Sign(player.X - X);
        if (side == 0) {
            side = -1;
        }

        BadelineDummy badeline = new(Position);
        Scene.Add(badeline);
        if (side == -1) {
            player.Facing = Facings.Right;
        } else {
            player.Facing = Facings.Left;
        }

        badeline.Sprite.Scale.X = side;
        Vector2 playerFrom = player.Position;
        Vector2 playerTo = Position + new Vector2(side * 4, -3f);
        Vector2 badelineFrom = badeline.Position;
        Vector2 badelineTo = Position + new Vector2(-side * 4, 3f);
        for (var p = 0.0f; (double) p < 1.0; p += Engine.DeltaTime / 0.2f) {
            Vector2 target = Vector2.Lerp(playerFrom, playerTo, p);
            if (player.Scene != null) {
                player.MoveToX(target.X);
            }

            if (player.Scene != null) {
                player.MoveToY(target.Y);
            }

            badeline.Position = Vector2.Lerp(badelineFrom, badelineTo, p);
            yield return null;
        }

        if (finalBoost) {
            Vector2 center = new Vector2(Calc.Clamp(player.X - level.Camera.X, 120f, 200f),
                Calc.Clamp(player.Y - level.Camera.Y, 60f, 120f));
            Add(new Coroutine(level.ZoomTo(center, 1.5f, 0.18f)));
            Engine.TimeRate = 0.5f;
        } else {
            Audio.Play("event:/char/badeline/booster_throw", Position);
        }

        badeline.Sprite.Play("boost");
        yield return 0.1f;
        if (!player.Dead) {
            player.MoveV(5f);
        }

        yield return 0.1f;

        if (finalBoost) {
            List<Follower> followers = player.Leader.Followers;
            if (!followers.Any(follower => follower.Entity is Key)) {
                infos.RemoveAll(info => info.Collectable == Collectable.Key);
            }
            if (!followers.Any(follower => follower.Entity is Strawberry {Golden: true})) {
                infos.RemoveAll(info => info.Collectable == Collectable.Golden);
            }
            if (!followers.Any(follower => follower.Entity is Strawberry {Moon: true})) {
                infos.RemoveAll(info => info.Collectable == Collectable.Moon);
            }

            if (infos.Count > 0) {
                Scene.Add(new CS_BoostTeleport(player, this, infos.First()));
                player.Active = false;
                badeline.Active = false;
                Active = false;
                yield return null;
                player.Active = true;
                badeline.Active = true;
            }
        }

        Add(Alarm.Create(Alarm.AlarmMode.Oneshot, () => {
            if (player.Dashes < player.Inventory.Dashes) {
                ++player.Dashes;
            }

            Scene.Remove(badeline);
            level.Displacement.AddBurst(badeline.Position, 0.25f, 8f, 32f, 0.5f);
        }, 0.15f, true));
        level.Shake();
        holding = null;
        if (!finalBoost) {
            player.BadelineBoostLaunch(CenterX);
            Vector2 from = Position;
            Vector2 to = nodes[nodeIndex];
            var time = Vector2.Distance(from, to) / MoveSpeed;
            time = Math.Min(3f, time);
            stretch.Visible = true;
            stretch.Rotation = (to - from).Angle();
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, time, true);
            tween.OnUpdate = t => {
                Position = Vector2.Lerp(from, to, t.Eased);
                stretch.Scale.X = (float) (1.0 + Calc.YoYo(t.Eased) * 2.0);
                stretch.Scale.Y = (float) (1.0 - Calc.YoYo(t.Eased) * 0.75);
                if (t.Eased >= 0.899999976158142 || !Scene.OnInterval(0.03f)) {
                    return;
                }

                TrailManager.Add(this, Player.TwoDashesHairColor, 0.5f, false);
                level.ParticlesFG.Emit(BadelineBoost.P_Move, 1, Center, Vector2.One * 4f);
            };
            tween.OnComplete = t => {
                if (X >= (double) level.Bounds.Right) {
                    RemoveSelf();
                } else {
                    travelling = false;
                    stretch.Visible = false;
                    sprite.Visible = true;
                    Collidable = true;
                    Audio.Play("event:/char/badeline/booster_reappear", Position);
                }
            };
            Add(tween);
            relocateSfx.Play("event:/char/badeline/booster_relocate");
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            level.DirectionalShake(-Vector2.UnitY);
            level.Displacement.AddBurst(Center, 0.4f, 8f, 32f, 0.5f);
        } else {
            Ch9FinalBoostSfx = Audio.Play("event:/new_content/char/badeline/booster_finalfinal_part2", Position);
            Engine.FreezeTimer = 0.1f;
            yield return null;
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
            level.Flash(Color.White * 0.5f, true);
            level.DirectionalShake(-Vector2.UnitY, 0.6f);
            level.Displacement.AddBurst(Center, 0.6f, 8f, 64f, 0.5f);
            level.ResetZoom();
            player.SummitLaunch(X);
            Engine.TimeRate = 1f;
            Finish();
        }
    }

    public override void Update() {
        if (sprite.Visible && Scene.OnInterval(0.05f)) {
            SceneAs<Level>().ParticlesBG.Emit(BadelineBoost.P_Ambience, 1, Center, Vector2.One * 3f);
        }

        if (holding != null) {
            holding.Speed = Vector2.Zero;
        }

        if (!travelling) {
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity != null) {
                var num = Calc.ClampedMap((entity.Center - Position).Length(), 16f, 64f, 12f, 0.0f);
                sprite.Position = Calc.Approach(sprite.Position, (entity.Center - Position).SafeNormalize() * num,
                    32f * Engine.DeltaTime);
            }
        }

        light.Visible = bloom.Visible = sprite.Visible || stretch.Visible;
        base.Update();
    }

    private void Finish() {
        SceneAs<Level>().Displacement.AddBurst(Center, 0.5f, 24f, 96f, 0.4f);
        SceneAs<Level>().Particles.Emit(BadelineOldsite.P_Vanish, 12, Center, Vector2.One * 6f);
        SceneAs<Level>().CameraLockMode = Level.CameraLockModes.None;
        SceneAs<Level>().CameraOffset = new Vector2(0.0f, -16f);
        RemoveSelf();
    }
}