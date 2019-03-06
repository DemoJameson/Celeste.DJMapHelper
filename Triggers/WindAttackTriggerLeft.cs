using Celeste.Mod.DJMapHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DJMapHelper.Triggers {
    [Tracked]
    public class WindAttackTriggerLeft : Trigger {
        public WindAttackTriggerLeft(EntityData data, Vector2 offset)
            : base(data, offset) { }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            if (Scene.Entities.FindFirst<SnowballLeft>() == null) {
                Scene.Add(new SnowballLeft());
            }

            RemoveSelf();
        }
    }
}