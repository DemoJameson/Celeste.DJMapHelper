using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DJMapHelper.Entities {
    public class PlaySprite : Entity {
        private string SpriteName;
        private string SpriteId;
        private bool flipX;
        private bool flipY;
        private Sprite Sprite;

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