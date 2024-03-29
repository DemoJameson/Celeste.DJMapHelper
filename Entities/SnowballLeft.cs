using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DJMapHelper.Entities; 

public class SnowballLeft : Entity {
    private const float ResetTime = 0.8f;
    private readonly SineWave sine;
    private readonly SoundSource spawnSfx;
    private readonly Sprite sprite;
    private float atY;
    private Level level;
    private float resetTimer;

    public SnowballLeft() {
        Depth = -12500;
        Collider = new Hitbox(12f, 9f, -6f, -2f);
        Collider bounceCollider = new Hitbox(16f, 6f, -9f, -8f);
        Add(
            new PlayerCollider(OnPlayer));
        Add(new PlayerCollider(OnPlayerBounce, bounceCollider));
        Add(sine = new SineWave(0.5f));
        Add(sprite = GFX.SpriteBank.Create("snowball"));
        sprite.Scale.X *= -1;
        sprite.Play("spin");
        Add(spawnSfx = new SoundSource());
    }

    public override void Added(Scene scene) {
        base.Added(scene);
        level = SceneAs<Level>();
        ResetPosition();
    }

    private void ResetPosition() {
        Player entity = level.Tracker.GetEntity<Player>();
        if (entity != null && entity.Left > (double) (level.Bounds.Left + 64)) {
            spawnSfx.Play("event:/game/04_cliffside/snowball_spawn");
            Collidable = Visible = true;
            resetTimer = 0.0f;
            X = level.Camera.Left - 10f;
            atY = Y = entity.CenterY;
            sine.Reset();
            sprite.Play("spin");
        }
        else {
            resetTimer = 0.05f;
        }
    }

    private void Destroy() {
        Collidable = false;
        sprite.Play("break");
    }

    private void OnPlayer(Player player) {
        player.Die(new Vector2(1f, 0.0f));
        Destroy();
        Audio.Play("event:/game/04_cliffside/snowball_impact", Position);
    }

    private void OnPlayerBounce(Player player) {
        if (CollideCheck(player)) {
            return;
        }

        Celeste.Freeze(0.1f);
        player.Bounce(Top - 2f);
        Destroy();
        Audio.Play("event:/game/general/thing_booped", Position);
    }

    public override void Update() {
        base.Update();
        X += 200f * Engine.DeltaTime;
        Y = atY + 4f * sine.Value;
        if (X <= level.Camera.Right + 60.0) {
            return;
        }

        resetTimer += Engine.DeltaTime;
        if (resetTimer >= ResetTime) {
            ResetPosition();
        }
    }

    public override void Render() {
        sprite.DrawOutline();
        base.Render();
    }
}