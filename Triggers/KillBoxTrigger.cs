using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DJMapHelper.Triggers; 

[Tracked]
[CustomEntity("DJMapHelper/killBoxTrigger")]
public class KillBoxTrigger : Trigger {
    public KillBoxTrigger(EntityData data, Vector2 offset) : base(data, offset) { }

    public override void OnEnter(Player player) {
        base.OnEnter(player);

        if (SaveData.Instance.Assists.Invincible || player.StateMachine.State == Player.StCassetteFly) {
            return;
        }

        player.Die((player.Position - Position).SafeNormalize());
    }
}