using System.Linq;
using System.Reflection;
using Celeste.Mod.DJMapHelper.Extensions;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DJMapHelper.Triggers {
    [Tracked]
    [CustomEntity("DJMapHelper/changeBossPatternTrigger")]
    public class ChangeBossPatternTrigger : Trigger {
        private static readonly FieldInfo PatternIndexFieldInfo = typeof(FinalBoss).GetPrivateField("patternIndex");
        private static readonly FieldInfo NormalHairFieldInfo = typeof(FinalBoss).GetPrivateField("normalHair");
        private static readonly FieldInfo NodesFieldInfo = typeof(FinalBoss).GetPrivateField("nodes");
        private static readonly FieldInfo AttackCoroutineFieldInfo = typeof(FinalBoss).GetPrivateField("attackCoroutine");
        private static readonly MethodInfo StartAttackingMethodInfo = typeof(FinalBoss).GetPrivateMethod("StartAttacking");
        private static readonly MethodInfo CreateBossSpriteMethodInfo = typeof(FinalBoss).GetPrivateMethod("CreateBossSprite");

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
                    if (patternIndex == (int) PatternIndexFieldInfo.GetValue(finalBoss)) continue;
                    if (((Vector2[]) NodesFieldInfo.GetValue(finalBoss)).Length == 0) continue;

                    TrySwitchSprite(finalBoss);
                    PatternIndexFieldInfo.SetValue(finalBoss, patternIndex);
                    if (patternIndex == 0) {
                        (AttackCoroutineFieldInfo.GetValue(finalBoss) as Coroutine)?.Cancel();
                    } else {
                        StartAttackingMethodInfo.Invoke(finalBoss, null);
                    }
                }
            }
        }

        private void TrySwitchSprite(FinalBoss finalBoss) {
            if (patternIndex == 0) {
                PlayerSprite normalSprite = new PlayerSprite(PlayerSpriteMode.Badeline) {Scale = {X = -1f}};
                normalSprite.Play("laugh");

                PlayerHair normalHair = new PlayerHair(normalSprite) {
                    Color = BadelineOldsite.HairColor, Border = Color.Black, Facing = Facings.Left
                };

                finalBoss.NormalSprite = normalSprite;
                NormalHairFieldInfo.SetValue(finalBoss, normalHair);

                finalBoss.Add(normalHair);
                finalBoss.Add(normalSprite);

                if (finalBoss.Sprite != null) {
                    normalSprite.Position = finalBoss.Sprite.Position;
                    finalBoss.Remove(finalBoss.Sprite);
                    if (finalBoss.Get<SpriteRemovedComponent>() == null) {
                        finalBoss.Add(new SpriteRemovedComponent());
                    }
                    // 许多方法需要 Sprite 所以暂时设置为 null，等到 OnPlayer 之时才设置为 null
                    // finalBoss.Sprite = null;
                }
            } else {
                CreateBossSpriteMethodInfo?.Invoke(finalBoss, null);
            }
        }

        public class SpriteRemovedComponent : Component {
            public SpriteRemovedComponent() : base(false, false) { }
        }

        public static void OnLoad() {
            On.Celeste.FinalBoss.OnPlayer += FinalBossOnOnPlayer;
        }

        public static void OnUnload() {
            On.Celeste.FinalBoss.OnPlayer -= FinalBossOnOnPlayer;
        }

        private static void FinalBossOnOnPlayer(On.Celeste.FinalBoss.orig_OnPlayer orig, FinalBoss self, Player player) {
            if (self.Get<SpriteRemovedComponent>() is Component component) {
                self.Sprite = null;
                self.Remove(component);
            }
            orig(self, player);
        }
    }
}