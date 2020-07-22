using Celeste.Mod.DJMapHelper.Entities;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DJMapHelper.Triggers {
    [Tracked]
    [CustomEntity("DJMapHelper/badelineProtectTrigger")]
    public class BadelineProtectTrigger : Trigger {
        private readonly EntityData data;
        private readonly Vector2 offset;

        public BadelineProtectTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            this.data = data;
            this.offset = offset;
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);
            Scene.Add(new BadelineProtector(data, offset));
            RemoveSelf();
        }
    }
}