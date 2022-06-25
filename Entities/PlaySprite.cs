using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DJMapHelper.Entities; 

[CustomEntity("DJMapHelper/playSprite")]
public class PlaySprite : Entity {
    public PlaySprite(EntityData data, Vector2 offset) : base(data.Position + offset) {
        string spriteName = data.Attr("SpriteName");
        string spriteId = data.Attr("SpriteId");
        bool flipX = data.Bool("flipX");
        bool flipY = data.Bool("flipY");

        Sprite sprite;
        Add(sprite = GFX.SpriteBank.Create(spriteName));
        if (spriteId != "") {
            sprite.Play(spriteId);
        }

        if (flipX) {
            sprite.Scale.X *= -1;
        }

        if (flipY) {
            sprite.Scale.Y *= -1;
        }
    }
}