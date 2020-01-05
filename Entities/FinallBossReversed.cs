using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DJMapHelper.Entities {
    public class FinalBossReversed : Entity {
        private readonly FinalBoss finalBoss;
        private List<Entity> fallingBlocks;

        public FinalBossReversed(EntityData data, Vector2 offset) {
            finalBoss = new FinalBoss(data, offset) {new ReverseComponent(this)};
            finalBoss.Remove(finalBoss.Get<CameraLocker>());
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            scene.Add(finalBoss);
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            fallingBlocks = Scene.Tracker.GetEntitiesCopy<FallingBlock>();
            fallingBlocks.Sort((a, b) => (int) ((double) b.X - (double) a.X));
        }

        public static void OnLoad() {
            On.Celeste.FinalBoss.TriggerFallingBlocks += FinalBossOnTriggerFallingBlocks;
        }

        public static void OnUnload() {
            On.Celeste.FinalBoss.TriggerFallingBlocks -= FinalBossOnTriggerFallingBlocks;
        }

        private static void FinalBossOnTriggerFallingBlocks(On.Celeste.FinalBoss.orig_TriggerFallingBlocks orig,
            FinalBoss self, float badelineX) {
            ReverseComponent reverseComponent = self.Get<ReverseComponent>();
            if (reverseComponent != null) {
                var fallingBlocks = reverseComponent.FinalBossReversed.fallingBlocks;
                while (fallingBlocks.Count > 0 && fallingBlocks[0].Scene == null) fallingBlocks.RemoveAt(0);

                var num = 0;
                while (fallingBlocks.Count > 0 && fallingBlocks[0].X > badelineX) {
                    if (!(fallingBlocks[0] is FallingBlock fallingBlock)) continue;

                    fallingBlock.StartShaking();
                    fallingBlock.Triggered = true;
                    fallingBlock.FallDelay = 0.4f * num;
                    ++num;
                    fallingBlocks.RemoveAt(0);
                }
            }
            else {
                orig(self, badelineX);
            }
        }

        private class ReverseComponent : Component {
            public readonly FinalBossReversed FinalBossReversed;

            public ReverseComponent(FinalBossReversed finalBossReversed) : base(true, true) {
                FinalBossReversed = finalBossReversed;
            }
        }
    }
}