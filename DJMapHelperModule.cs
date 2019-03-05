using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public override Type SettingsType => null;

        // If you don't need to store any save data, => null
        public override Type SaveDataType => null;


        // Set up any hooks, event handlers and your mod in general here.
        // Load runs before Celeste itself has initialized properly.
        public override void Load() {
            Everest.Events.Level.OnLoadEntity += LevelOnOnLoadEntity;

            ColorfulFlyFeather.OnLoad();
            ColorfulRefill.OnLoad();
            ClimbBlockerTrigger.OnLoad();
            FeatherBarrier.OnLoad();
            TheoCrystalIntoBubble.OnLoad();
            DestoryAllCrystalSpinnerTrigger.OnLoad();
            WindAttackTriggerLeft.OnLoad();
            MaxDashesTrigger.OnLoad();
            FinalBossReversed.OnLoad();
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

            ColorfulFlyFeather.OnUnload();
            ColorfulRefill.OnUnload();
            ClimbBlockerTrigger.OnUnLoad();
            FeatherBarrier.OnUnload();
            TheoCrystalIntoBubble.OnUnload();
            DestoryAllCrystalSpinnerTrigger.OnUnload();
            WindAttackTriggerLeft.OnUnload();
            MaxDashesTrigger.OnUnload();
            FinalBossReversed.OnUnload();
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
                case "climbBlockerTrigger":
                    level.Add(new ClimbBlockerTrigger(entityData, offset));
                    return true;
                case "colorfulFlyFeather":
                    level.Add(new ColorfulFlyFeather(entityData, offset));
                    return true;
                case "colorfulRefill":
                    level.Add(new ColorfulRefill(entityData, offset));
                    return true;
                case "featherBarrier":
                    level.Add(new FeatherBarrier(entityData, offset));
                    return true;
                case "templeGateReversed":
                    level.Add(new TempleGateReversed(entityData, offset));
                    return true;
                case "maxDashesTrigger":
                    level.Add(new MaxDashesTrigger(entityData, offset));
                    return true;
                case "windAttackTriggerLeft":
                    level.Add(new WindAttackTriggerLeft(entityData, offset));
                    return true;
                case "destoryAllCrystalSpinnerTrigger":
                    level.Add(new DestoryAllCrystalSpinnerTrigger(entityData, offset));
                    return true;
                case "finalBossReversed":
                    level.Add(new FinalBossReversed(entityData, offset));
                    return true;
                case "killBoxTrigger":
                    level.Add(new KillBoxTrigger(entityData, offset));
                    return true;
                case "theoCrystalBarrier":
                    level.Add(new TheoCrystalBarrier(entityData, offset));
                    return true;
            }

            return false;
        }
    }
}