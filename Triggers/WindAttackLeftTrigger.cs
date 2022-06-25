using Celeste.Mod.DJMapHelper.Entities;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DJMapHelper.Triggers; 

[Tracked]
[CustomEntity("DJMapHelper/windAttackTriggerLeft")]
public class WindAttackLeftTrigger : Trigger {
    public WindAttackLeftTrigger(EntityData data, Vector2 offset)
        : base(data, offset) { }

    public override void OnEnter(Player player) {
        base.OnEnter(player);

        if (Scene.Entities.FindFirst<SnowballLeft>() == null) {
            Scene.Add(new SnowballLeft());
        }

        RemoveSelf();
    }
}