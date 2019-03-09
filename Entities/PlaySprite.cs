using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
namespace Celeste.Mod.DJMapHelper.Entities
{
    public class PlaySprite : Entity
    {
        private string SpriteName;
        private string SpriteId;
        private bool flipX;
        private bool flipY;
        public Sprite Sprite;

        PlaySprite(Vector2 position)
            :base(position)
        {
        }

        public PlaySprite(EntityData data, Vector2 offset)
            : this(data.Position + offset)
        {
            SpriteName = data.Attr("SpriteName");
            SpriteId = data.Attr("SpriteId");
            flipX = data.Bool("flipX");
            flipY = data.Bool("filpY");
            Add((Component) (Sprite = GFX.SpriteBank.Create(SpriteName)));
            if(SpriteId!="")Sprite.Play(SpriteId);
            if(flipX)Sprite.Scale.X *= -1;
            if(flipY)Sprite.Scale.Y *= -1;
        }
    }
}