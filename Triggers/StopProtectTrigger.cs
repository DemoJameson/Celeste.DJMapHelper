using Celeste.Mod.DJMapHelper.Entities;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.DJMapHelper.Triggers; 

[CustomEntity("DJMapHelper/StopProtectTrigger")]
public class StopProtectTrigger : Trigger {
    public StopProtectTrigger(EntityData data, Vector2 offset) : base(data, offset) { }

    public override void OnEnter(Player player) {
        base.OnEnter(player);

        foreach (BadelineProtector protector in Scene.Entities.FindAll<BadelineProtector>()) {
            protector.RemoveSelf();
        }

        RemoveSelf();
    }
}