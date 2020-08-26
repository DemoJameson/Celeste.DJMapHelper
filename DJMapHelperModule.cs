using System;
using Celeste.Mod.DJMapHelper.DebugMode;
using Celeste.Mod.DJMapHelper.Entities;
using Celeste.Mod.DJMapHelper.Triggers;
using Monocle;

// TODO: 添加 Ahorn 插件的语言文件（Tooltip）
namespace Celeste.Mod.DJMapHelper {
    // ReSharper disable once ClassNeverInstantiated.Global
    public class DJMapHelperModule : EverestModule {
        // Only one alive module instance can exist at any given time.
        // ReSharper disable once NotAccessedField.Global
        public static DJMapHelperModule Instance;

        public DJMapHelperModule() {
            Instance = this;
        }

        // If you don't need to store any settings, => null
        public override Type SettingsType => typeof(DJMapHelperSettings);
        public static DJMapHelperSettings Settings => (DJMapHelperSettings) Instance._Settings;

        // If you don't need to store any save data, => null
        public override Type SaveDataType => null;


        // Set up any hooks, event handlers and your mod in general here.
        // Load runs before Celeste itself has initialized properly.
        public override void Load() {
            BadelineBoostDown.OnLoad();
            ChangeBossPatternTrigger.OnLoad();
            ChangeSpinnerColorTrigger.OnLoad();
            ColorfulFlyFeather.OnLoad();
            ColorfulRefill.OnLoad();
            ClimbBlockerTrigger.OnLoad();
            FeatherBarrier.OnLoad();
            FinalBossReversed.OnLoad();
            FlingBirdReversed.OnLoad();
            LookoutBuilder.OnLoad();
            TheoCrystalBarrier.OnLoad();
            SpringGreen.OnLoad();
        }

        // Unload the entirety of your mod's content, remove any event listeners and undo all hooks.
        public override void Unload() {
            BadelineBoostDown.OnUnLoad();
            ChangeBossPatternTrigger.OnUnload();
            ChangeSpinnerColorTrigger.OnUnload();
            ColorfulFlyFeather.OnUnload();
            ColorfulRefill.OnUnload();
            ClimbBlockerTrigger.OnUnLoad();
            FeatherBarrier.OnUnload();
            FinalBossReversed.OnUnload();
            FlingBirdReversed.OnUnLoad();
            LookoutBuilder.OnUnload();
            TheoCrystalBarrier.OnUnload();
            SpringGreen.OnUnLoad();
        }
    }
}