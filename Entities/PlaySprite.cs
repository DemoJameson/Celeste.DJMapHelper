using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DJMapHelper.Entities {
    public class PlaySprite : Entity {
        private readonly bool flipX;
        private readonly bool flipY;
        private readonly Sprite Sprite;
        private readonly string SpriteId;
        private readonly string SpriteName;

        public PlaySprite(EntityData data, Vector2 offset) : base(data.Position + offset) {
            SpriteName = data.Attr("SpriteName");
            SpriteId = data.Attr("SpriteId");
            flipX = data.Bool("flipX");
            flipY = data.Bool("filpY");
            Add(Sprite = GFX.SpriteBank.Create(SpriteName));
            if (SpriteId != "") Sprite.Play(SpriteId);
            if (flipX) Sprite.Scale.X *= -1;
            if (flipY) Sprite.Scale.Y *= -1;
        }
    }
}