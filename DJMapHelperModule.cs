using System;
using Celeste.Mod.DJMapHelper.DebugMode;
using Celeste.Mod.DJMapHelper.Entities;
using Celeste.Mod.DJMapHelper.Triggers;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DJMapHelper {
    // ReSharper disable once ClassNeverInstantiated.Global
    public class DJMapHelperModule : EverestModule {
        public SpriteBank SpriteBank { get; set; }


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
            Everest.Events.Level.OnLoadEntity += LevelOnOnLoadEntity;

            // BarrierUtils.OnLoad();
            ColorfulFlyFeather.OnLoad();
            ColorfulRefill.OnLoad();
            ClimbBlockerTrigger.OnLoad();
            FeatherBarrier.OnLoad();
            FinalBossReversed.OnLoad();
            FlingBirdReversed.OnLoad();
            LookoutBuilder.OnLoad();
            ChangeSpinnerColorTrigger.OnLoad();
            TheoCrystalBarrier.OnLoad();
        }

        public override void LoadContent(bool firstLoad) {
            if (firstLoad) {
                SpriteBank = new SpriteBank(GFX.Game, "Graphics/DJMapHelperSprites.xml");
            }
        }

        // Unload the entirety of your mod's content, remove any event listeners and undo all hooks.
        public override void Unload() {
            Everest.Events.Level.OnLoadEntity -= LevelOnOnLoadEntity;
            
            // BarrierUtils.OnUnLoad();
            ColorfulFlyFeather.OnUnload();
            ColorfulRefill.OnUnload();
            ClimbBlockerTrigger.OnUnLoad();
            FeatherBarrier.OnUnload();
            FinalBossReversed.OnUnload();
            FlingBirdReversed.OnUnLoad();
            LookoutBuilder.OnUnload();
            ChangeSpinnerColorTrigger.OnUnload();
            TheoCrystalBarrier.OnUnload();
        }


        private static bool LevelOnOnLoadEntity(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            const string prefix = "DJMapHelper/";
            string entityName = entityData.Name;
            if (!entityName.StartsWith(prefix)) {
                return false;
            }

            entityName = entityName.Remove(0, prefix.Length);

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (entityName) {
                // Entities
                case "colorfulFlyFeather":
                    level.Add(new ColorfulFlyFeather(entityData, offset));
                    return true;
                case "colorfulRefill":
                    level.Add(new ColorfulRefill(entityData, offset));
                    return true;
                case "featherBarrier":
                    level.Add(new FeatherBarrier(entityData, offset));
                    return true;
                case "flingBirdReversed":
                    level.Add(new FlingBirdReversed(entityData, offset));
                    return true;
                case "templeGateReversed":
                    level.Add(new TempleGateReversed(entityData, offset));
                    return true;
                case "finalBossReversed":
                    level.Add(new FinalBossReversed(entityData, offset));
                    return true;
                case "startPoint":
                    level.Add(new StartPoint(entityData, offset));
                    return true;
                case "theoCrystalBarrier":
                    level.Add(new TheoCrystalBarrier(entityData, offset));
                    return true;
                case "oshiroBossRight":
                    level.Add(new AngryOshiroRight(entityData, offset));
                    return true;
                case "playSprite":
                    level.Add(new PlaySprite(entityData, offset));
                    return true;

                // Triggers
                case "changeBossPatternTrigger":
                    level.Add(new ChangeBossPatternTrigger(entityData, offset));
                    return true;
                case "changeSpinnerColorTrigger":
                    level.Add(new ChangeSpinnerColorTrigger(entityData, offset));
                    return true;
                case "climbBlockerTrigger":
                    level.Add(new ClimbBlockerTrigger(entityData, offset));
                    return true;
                case "colorGradeTrigger":
                    level.Add(new ColorGradeTrigger(entityData, offset));
                    return true;
                case "killBoxTrigger":
                    level.Add(new KillBoxTrigger(entityData, offset));
                    return true;
                case "maxDashesTrigger":
                    level.Add(new MaxDashesTrigger(entityData, offset));
                    return true;
                case "talkToBadelineTrigger":
                    level.Add(new TalkToBadelineTrigger(entityData, offset));
                    return true;
                case "teleportTrigger":
                    level.Add(new TeleportTrigger(entityData, offset));
                    return true;
                case "windAttackTriggerLeft":
                    level.Add(new WindAttackLeftTrigger(entityData, offset));
                    return true;
            }

            return false;
        }
    }
}