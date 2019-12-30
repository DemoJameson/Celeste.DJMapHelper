using Monocle;
using System;
namespace Celeste.Mod.DJMapHelper.Colliders
{
    [Tracked(false)]
    public class TouchSwitchCollider: Component
    {
        public Action<TouchSwitch> OnCollide;
        public Collider Collider;

        public TouchSwitchCollider(Action<TouchSwitch> onCollide, Collider collider = null):base(false, false)
        {
            OnCollide = onCollide;
            Collider = collider;
        }

        public void Check(TouchSwitch touch)
        {
            if (OnCollide == null)
                return;
            Collider collider = Entity.Collider;
            if (Collider != null)
                Entity.Collider = Collider;
            if (!touch.CollideCheck(Entity))
                return;
            OnCollide(touch);
            Entity.Collider = collider;
        }
    }
}