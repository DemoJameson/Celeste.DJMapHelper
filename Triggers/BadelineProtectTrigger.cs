using Celeste.Mod.DJMapHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DJMapHelper.Triggers {
    [Tracked]
    public class BadelineProtectTrigger : Trigger {
        public BadelineProtectTrigger(EntityData data, Vector2 offset) : base(data, offset) { }

        public override void OnEnter(Player player) {
            base.OnEnter(player);
            Scene.Add(new BadelineProtector());
            RemoveSelf();
        }
    }
}