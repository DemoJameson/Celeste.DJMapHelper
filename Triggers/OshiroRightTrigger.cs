using Celeste.Mod.DJMapHelper.Entities;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.DJMapHelper.Triggers {
    [CustomEntity("DJMapHelper/oshiroRightTrigger")]
    public class OshiroRightTrigger : Trigger {
        public bool State;

        public OshiroRightTrigger(EntityData data, Vector2 offset)
            : base(data, offset) {
            State = data.Bool("state", defaultValue: true);
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);
            if (State) {
                Level level = SceneAs<Level>();
                Vector2 position = new Vector2(level.Bounds.Left - 32, level.Bounds.Top + level.Bounds.Height / 2);
                level.Add(new AngryOshiroRight(position));
            } else {
                Scene.Tracker.GetEntity<AngryOshiroRight>()?.Leave();
            }
            RemoveSelf();
        }
    }
}