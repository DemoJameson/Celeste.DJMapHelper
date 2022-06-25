using Celeste.Mod.DJMapHelper.Entities;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DJMapHelper.Triggers; 

[Tracked]
[CustomEntity("DJMapHelper/badelineProtectTrigger")]
public class BadelineProtectTrigger : Trigger {
    private readonly EntityData data;

    public BadelineProtectTrigger(EntityData data, Vector2 offset) : base(data, offset) {
        this.data = data;
    }

    public override void OnEnter(Player player) {
        base.OnEnter(player);
        if (Scene.Tracker.GetEntity<BadelineProtector>() is BadelineProtector badelineProtector) {
            badelineProtector.RemoveSelf();
            DJMapHelperModule.Session.BadelineProtectorConfig = null;
        }

        if (data.Int("maxQuantity") > 0) {
            Scene.Add(new BadelineProtector(data));
        }

        EntityID entityId = new EntityID(data.Level.Name, data.ID + 10000000);
        SceneAs<Level>().Session.DoNotLoad.Add(entityId);
        RemoveSelf();
    }
}