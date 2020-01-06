using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DJMapHelper.Entities {
    public class Shield : Entity {
        public static ParticleType P_Collect;
        public static ParticleType P_Boost;
        public static ParticleType P_Flying;
        public static ParticleType P_Respawn;
        private readonly BloomPoint bloom;
        private readonly VertexLight light;
        private readonly Wiggler moveWiggle;
        private readonly Image outline;
        private readonly Wiggler shieldRadiusWiggle;
        private readonly SineWave sine;
        private readonly Sprite sprite;
        private Level level;
        private Vector2 moveWiggleDir;
        private Wiggler wiggler;

        public Shield(Vector2 position) : base(position) {
            Collider = new Circle(10f);
            Add(new PlayerCollider(OnPlayer));
            Add(sprite = GFX.SpriteBank.Create("flyFeather"));
            Add(wiggler = Wiggler.Create(1f, 4f,
                v => sprite.Scale = Vector2.One * (float) (1.0 + (double) v * 0.200000002980232)));
            Add(bloom = new BloomPoint(0.5f, 20f));
            Add(light = new VertexLight(Color.White, 1f, 16, 48));
            Add(sine = new SineWave(0.6f, 0.0f).Randomize());
            Add(outline = new Image(GFX.Game["objects/flyFeather/outline"]));
            outline.CenterOrigin();
            outline.Visible = false;
            sprite.Visible = false;
            shieldRadiusWiggle = Wiggler.Create(0.5f, 4f);
            Add(shieldRadiusWiggle);
            moveWiggle = Wiggler.Create(0.8f, 2f);
            moveWiggle.StartZero = true;
            Add(moveWiggle);
            UpdateY();
        }

        public Shield(EntityData data, Vector2 offset) : this(data.Position + offset) { }

        public override void Added(Scene scene) {
            base.Added(scene);
            level = SceneAs<Level>();
        }

        public override void Update() {
            base.Update();
            UpdateY();
            light.Alpha = Calc.Approach(light.Alpha, sprite.Visible ? 1f : 0.0f, 4f * Engine.DeltaTime);
            bloom.Alpha = light.Alpha * 0.8f;
        }

        public override void Render() {
            base.Render();
            Draw.Circle(Position + sprite.Position, (float) (10.0 - shieldRadiusWiggle.Value * 2.0), Color.White, 3);
        }

        private void UpdateY() {
            this.sprite.X = 0.0f;
            this.sprite.Y = bloom.Y = sine.Value * 2f;
            Sprite sprite = this.sprite;
            sprite.Position = sprite.Position + moveWiggleDir * moveWiggle.Value * -8f;
        }

        private void OnPlayer(Player player) {
            player.PointBounce(Center);
            if (Input.MoveX.Value == Math.Sign(player.Speed.X)) {
                player.Speed.X *= 1.2f;
            }

            moveWiggle.Start();
            shieldRadiusWiggle.Start();
            moveWiggleDir = (Center - player.Center).SafeNormalize(Vector2.UnitY);
            Audio.Play("event:/game/06_reflection/feather_bubble_bounce", Position);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            level.DirectionalShake((player.Center - Center).SafeNormalize(), 0.15f);
        }
    }
}