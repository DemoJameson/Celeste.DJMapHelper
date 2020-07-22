using System;
using Celeste.Mod.DJMapHelper.DebugMode;
using Celeste.Mod.DJMapHelper.Entities;
using Celeste.Mod.DJMapHelper.Triggers;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DJMapHelper {
    // ReSharper disable once ClassNeverInstantiated.Global
    public class DJMapHelperModule : EverestModule {
        // Only one alive module instance can exist at any given time.
        // ReSharper disable once NotAccessedField.Global
        public static DJMapHelperModule Instance;

        public DJMapHelperModule() {
            Instance = this;
        }

        public SpriteBank SpriteBank { get; set; }

        // If you don't need to store any settings, => null
        public override Type SettingsType => typeof(DJMapHelperSettings);
        public static DJMapHelperSettings Settings => (DJMapHelperSettings) Instance._Settings;

        // If you don't need to store any save data, => null
        public override Type SaveDataType => null;


        // Set up any hooks, event handlers and your mod in general here.
        // Load runs before Celeste itself has initialized properly.
        public override void Load() {
            ColorfulFlyFeather.OnLoad();
            ColorfulRefill.OnLoad();
            ClimbBlockerTrigger.OnLoad();
            FeatherBarrier.OnLoad();
            FinalBossReversed.OnLoad();
            FlingBirdReversed.OnLoad();
            LookoutBuilder.OnLoad();
            ChangeSpinnerColorTrigger.OnLoad();
            TheoCrystalBarrier.OnLoad();
            SpringGreen.OnLoad();
            BadelineBoostDown.OnLoad();
        }

        public override void LoadContent(bool firstLoad) {
            if (firstLoad) {
                SpriteBank = new SpriteBank(GFX.Game, "Graphics/DJMapHelperSprites.xml");
            }
        }

        // Unload the entirety of your mod's content, remove any event listeners and undo all hooks.
        public override void Unload() {
            ColorfulFlyFeather.OnUnload();
            ColorfulRefill.OnUnload();
            ClimbBlockerTrigger.OnUnLoad();
            FeatherBarrier.OnUnload();
            FinalBossReversed.OnUnload();
            FlingBirdReversed.OnUnLoad();
            LookoutBuilder.OnUnload();
            ChangeSpinnerColorTrigger.OnUnload();
            TheoCrystalBarrier.OnUnload();
            SpringGreen.OnUnLoad();
            BadelineBoostDown.OnUnLoad();
        }
    }
}