using System;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Reflection;
using Celeste;

namespace Celeste.Mod.DJMapHelper.Triggers {
    public class ChangeSpinnerColorTrigger : Trigger {
        public static void OnLoad() {
            On.Celeste.CrystalStaticSpinner.ctor_EntityData_Vector2_CrystalColor += ChangeColor;
            On.Celeste.LevelLoader.ctor += LevelLoaderOnCtor;
        }

        public static void OnUnload() {
            On.Celeste.CrystalStaticSpinner.ctor_EntityData_Vector2_CrystalColor -= ChangeColor;
            On.Celeste.LevelLoader.ctor -= LevelLoaderOnCtor;
        }

        private static void ChangeColor(On.Celeste.CrystalStaticSpinner.orig_ctor_EntityData_Vector2_CrystalColor orig,
            CrystalStaticSpinner self, EntityData data, Vector2 offset, CrystalColor color) {
            switch (Color) {
                case "Blue":
                    orig(self, data, offset, CrystalColor.Blue);
                    break;
                case "Red":
                    orig(self, data, offset, CrystalColor.Red);
                    break;
                case "Purple":
                    orig(self, data, offset, CrystalColor.Purple);
                    break;
                case "Rainbow":
                    orig(self, data, offset, CrystalColor.Rainbow);
                    break;
                default:
                    orig(self, data, offset, color);
                    break;
            }
        }

        private static void LevelLoaderOnCtor(On.Celeste.LevelLoader.orig_ctor orig, LevelLoader self, Session session, Vector2? startPosition) {
            Color = "Default";
            // 将 trigger 提前到所有刺初始化之前，这样才能应用修改的颜色
            var entityDataList = session.LevelData.Triggers.FindAll(data => data.Name == "DJMapHelper/changeSpinnerColorTrigger");
            foreach (EntityData entityData in entityDataList) {
                session.LevelData.Triggers.Remove(entityData);
                session.LevelData.Entities.Insert(0, entityData);
            }
            orig(self, session, startPosition);
        }

        private static string Color = "Default";
        private readonly string color;
        private readonly Modes mode;

        public ChangeSpinnerColorTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            mode = data.Enum<Modes>("mode", Modes.OnPlayerEnter);
            color = data.Attr("color", "Default");

            if (mode == Modes.OnLevelStart) {
                Color = color;
                RemoveSelf();
            }
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);
            if (mode == Modes.OnPlayerEnter) {
                Color = color;
                RemoveSelf();
            }
        }

        private enum Modes {
            OnPlayerEnter,
            OnLevelStart
        }
    }
}