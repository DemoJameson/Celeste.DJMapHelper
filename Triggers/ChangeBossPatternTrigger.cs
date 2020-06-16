using System.Linq;
using System.Reflection;
using Celeste.Mod.DJMapHelper.Extensions;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DJMapHelper.Triggers {
    [Tracked]
    public class ChangeBossPatternTrigger : Trigger {
        private static readonly FieldInfo PatternIndexFieldInfo = typeof(FinalBoss).GetPrivateField("patternIndex");
        private static readonly MethodInfo StartAttackingMethodInfo = typeof(FinalBoss).GetPrivateMethod("StartAttacking");

        public enum Modes {
            Contained,
            All
        }

        private readonly bool dashless;
        private readonly Modes mode;
        private readonly int patternIndex;

        public ChangeBossPatternTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            mode = data.Enum("mode", Modes.All);
            dashless = data.Bool("dashless");
            patternIndex = data.Int("patternIndex", 1);
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            if (!(Engine.Scene is Level level)) {
                RemoveSelf();
                return;
            }

            if (dashless && (level.Session.Dashes != 0 || !level.Session.StartedFromBeginning)) {
                RemoveSelf();
                return;
            }

            var bosses = level.Tracker.GetEntities<FinalBoss>().Cast<FinalBoss>().ToList();

            foreach (FinalBoss finalBoss in bosses) {
                if (mode == Modes.All || CollideCheck(finalBoss)) {
                    PatternIndexFieldInfo?.SetValue(finalBoss, patternIndex);
                    StartAttackingMethodInfo?.Invoke(finalBoss, null);
                }
            }
        }
    }
}