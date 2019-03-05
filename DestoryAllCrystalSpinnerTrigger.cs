using Microsoft.Xna.Framework;
using Monocle;
namespace Celeste.Mod.DJMapHelper
{
    public class DestoryAllCrystalSpinnerTrigger: Trigger
    {
        public DestoryAllCrystalSpinnerTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
        }

        public static void OnLoad()
        {
            On.Celeste.Trigger.OnEnter += PlayerEnterDestoryAllCrystalSpinnerTrigger;
        }

        public static void OnUnload()
        {
            On.Celeste.Trigger.OnEnter -= PlayerEnterDestoryAllCrystalSpinnerTrigger;
        }
        private static void  PlayerEnterDestoryAllCrystalSpinnerTrigger(On.Celeste.Trigger.orig_OnEnter orig, Trigger self,
            Player player)
        {
            if (self.GetType() == typeof(DestoryAllCrystalSpinnerTrigger))
            {
                Audio.Play("event:/game/06_reflection/boss_spikes_burst");
                foreach (CrystalStaticSpinner entity in self.Scene.Tracker.GetEntities<CrystalStaticSpinner>())
                    entity.Destroy(true);
                Audio.SetMusic((string) null, true, true);
                self.RemoveSelf();
            }
            orig(self, player);
        }
    }
}