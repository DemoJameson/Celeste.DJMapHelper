using Microsoft.Xna.Framework;
using Monocle;
namespace Celeste.Mod.DJMapHelper
{
    public class WindAttackTriggerLeft: Trigger
    {
        public WindAttackTriggerLeft(EntityData data, Vector2 offset)
            : base(data, offset)
        {
        }

        public static void OnLoad()
        {
            On.Celeste.Trigger.OnEnter += PlayerEnterWindAttackTriggerLeft;
        }

        public static void OnUnload()
        {
            On.Celeste.Trigger.OnEnter -= PlayerEnterWindAttackTriggerLeft;
        }
        private static void  PlayerEnterWindAttackTriggerLeft(On.Celeste.Trigger.orig_OnEnter orig, Trigger self,
            Player player)
        {
            if (self.GetType() == typeof(WindAttackTriggerLeft))
            {
                if (self.Scene.Entities.FindFirst<SnowballLeft>() == null)
                    self.Scene.Add((Entity) new SnowballLeft());
                self.RemoveSelf();
            }

            orig(self, player);
        }
    }
}