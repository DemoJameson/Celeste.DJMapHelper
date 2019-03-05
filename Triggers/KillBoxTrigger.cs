using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DJMapHelper.Triggers {
    [Tracked]
    public class KillBoxTrigger : Trigger {
        public KillBoxTrigger(EntityData data, Vector2 offset) : base(data, offset) {}

        public override void OnEnter(Player player) {
            base.OnEnter(player);
            player.Die((player.Position - Position).SafeNormalize());
        }
    }
}