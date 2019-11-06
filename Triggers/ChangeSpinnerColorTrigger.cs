using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Celeste.Mod.DJMapHelper.Extensions;
using Microsoft.Xna.Framework;
using Monocle;

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
            if (Color != null) {
                orig(self, data, offset, (CrystalColor) Color);
            }
            else {
                orig(self, data, offset, color);
            }

            self.SetExtendedDataValue("color", (CrystalColor?) color);
        }

        private static void LevelLoaderOnCtor(On.Celeste.LevelLoader.orig_ctor orig, LevelLoader self, Session session, Vector2? startPosition) {
            Color = null;
            // 将 trigger 提前到所有刺初始化之前，这样才能应用修改的颜色
            var entityDataList = session.LevelData.Triggers.FindAll(data => data.Name == "DJMapHelper/changeSpinnerColorTrigger");
            foreach (EntityData entityData in entityDataList) {
                session.LevelData.Triggers.Remove(entityData);
                session.LevelData.Entities.Insert(0, entityData);
            }

            orig(self, session, startPosition);
        }

        private static readonly FieldInfo ColorFieldInfo = typeof(CrystalStaticSpinner).GetPrivateField("color");
        private static readonly MethodInfo ClearSpritesMethodInfo = typeof(CrystalStaticSpinner).GetPrivateMethod("ClearSprites");
        private static readonly MethodInfo CreateSpritesMethodInfo = typeof(CrystalStaticSpinner).GetPrivateMethod("CreateSprites");
        private static CrystalColor? Color;
        private readonly CrystalColor? color;
        private readonly Modes mode;

        public ChangeSpinnerColorTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            mode = data.Enum("mode", Modes.OnPlayerEnter);
            color = TryGetCrystalColor(data.Attr("color", "Default"));

            if (mode == Modes.OnLevelStart) {
                Color = color;
            }
        }

        private static CrystalColor? TryGetCrystalColor(string color) {
            switch (color) {
                case "Blue":
                    return CrystalColor.Blue;
                case "Red":
                    return CrystalColor.Red;
                case "Purple":
                    return CrystalColor.Purple;
                case "Rainbow":
                    return CrystalColor.Rainbow;
                case "Default":
                    return null;
            }

            return null;
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            if (mode == Modes.OnLevelStart) {
                RemoveSelf();
            }
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);
            RemoveSelf();
            
            if (mode == Modes.OnPlayerEnter) {
                Level level = player.SceneAs<Level>();
                Color = color;
                level.Tracker.GetEntities<CrystalStaticSpinner>().Cast<CrystalStaticSpinner>().ToList().ForEach(entity => {
                    if (color != null) {
                        entity.Add(new ChangeColorComponent(entity, (CrystalColor) color));
                        return;
                    }

                    var origColor = entity.GetExtendedDataValue<CrystalColor?>("color");
                    Logger.Log("DJMapHelper", "origColor=" + origColor);
                    if (origColor != null) {
                        if (origColor == ~CrystalColor.Blue) {
                            origColor = level.CoreMode != Session.CoreModes.Cold ? CrystalColor.Red : CrystalColor.Blue;
                        }
                        entity.Add(new ChangeColorComponent(entity, (CrystalColor) origColor));
                    }
                });
            }
        }

        private class ChangeColorComponent : Component {
            private readonly CrystalStaticSpinner spinner;
            private readonly CrystalColor newColor;

            public ChangeColorComponent(CrystalStaticSpinner spinner, CrystalColor newColor)
                : base(true, false) {
                this.spinner = spinner;
                this.newColor = newColor;
            }

            public override void Update() {
                try {
                    ColorFieldInfo.SetValue(spinner, newColor);
                    ClearSpritesMethodInfo.Invoke(spinner, null);
                    CreateSpritesMethodInfo.Invoke(spinner, null);
                }
                catch (Exception) {
                    // ignored
                }

                RemoveSelf();
            }
        }

        private enum Modes {
            OnPlayerEnter,
            OnLevelStart
        }
    }
}