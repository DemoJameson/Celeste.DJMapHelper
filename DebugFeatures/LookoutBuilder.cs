using System.Collections;
using System.Linq;
using System.Reflection;
using Celeste.Mod.DJMapHelper.Extensions;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DJMapHelper.DebugFeatures {
    // Ctrl+Q: Add tower viewer.
    public static class LookoutBuilder {
        private static bool? savedInvincible;

        private static readonly MethodInfo InteractMethod = typeof(Lookout).GetPrivateMethod("Interact");
        private static readonly FieldInfo InteractingField = typeof(Lookout).GetPrivateField("interacting");

        public static void OnLoad() {
            On.Celeste.Level.Update += LevelOnUpdate;
            On.Celeste.Actor.OnGround_int += ActorOnOnGroundInt;
        }

        public static void OnUnload() {
            On.Celeste.Level.Update -= LevelOnUpdate;
            On.Celeste.Actor.OnGround_int -= ActorOnOnGroundInt;
        }

        private static bool ActorOnOnGroundInt(On.Celeste.Actor.orig_OnGround_int orig, Actor self, int downCheck) {
            if (self is Player
                && self.SceneAs<Level>() is Level level
                && level.Tracker.GetEntities<Lookout>().Any(entity => entity.Get<LookoutComponent>() != null)) {
                return true;
            }

            return orig(self, downCheck);
        }

        private static void LevelOnUpdate(On.Celeste.Level.orig_Update orig, Level level) {
            orig(level);

            if (level.Tracker.GetEntities<Lookout>().All(entity => entity.Get<LookoutComponent>() == null) && savedInvincible != null) {
                SaveData.Instance.Assists.Invincible = (bool) savedInvincible;
                savedInvincible = null;
            }

            Player player = level.Tracker.GetEntity<Player>();
            if (player == null) return;

            if (level.Paused || level.Transitioning || level.SkippingCutscene || level.InCutscene || player.Dead || player.StateMachine.State == Player.StDummy) {
                return;
            }

            if (DJMapHelperModule.Settings.SpawnTowerViewer.Pressed) {
                DJMapHelperModule.Settings.SpawnTowerViewer.ConsumePress();
                if (level.Tracker.GetEntities<Lookout>().Any(entity => entity.Get<LookoutComponent>() != null)) {
                    return;
                }
                
                Lookout lookout = new Lookout(new EntityData {Position = player.Position}, Vector2.Zero) {
                    new LookoutComponent()
                };
                lookout.Add(new Coroutine(Look(lookout)));
                level.Add(lookout);
                level.Tracker.GetEntitiesCopy<LookoutBlocker>().ForEach(level.Remove);
            }
        }

        private static IEnumerator Look(Lookout lookout) {
            Player player = Engine.Scene.Tracker.GetEntity<Player>();
            if (player?.Scene == null || player.Dead) {
                yield break;
            }
            InteractMethod?.Invoke(lookout, new object[] {player});
            savedInvincible = SaveData.Instance.Assists.Invincible;
            SaveData.Instance.Assists.Invincible = true;

            Level level = player.SceneAs<Level>();
            Level.CameraLockModes savedCameraLockMode = level.CameraLockMode;
            Vector2 savedCameraPosition = level.Camera.Position;
            level.CameraLockMode = Level.CameraLockModes.None;

            Entity underfootPlatform = player.CollideFirstOutside<FloatySpaceBlock>(player.Position + Vector2.UnitY);

            bool interacting = (bool) InteractingField?.GetValue(lookout);
            while (!interacting) {
                player.Position = lookout.Position;
                interacting = (bool) InteractingField?.GetValue(lookout);
                yield return null;
            }

            while (interacting) {
                player.Position = lookout.Position;
                interacting = (bool) InteractingField?.GetValue(lookout);
                yield return null;
            }

            lookout.Collidable = lookout.Visible = false;

            if (savedInvincible != null) {
                SaveData.Instance.Assists.Invincible = (bool) savedInvincible;
                savedInvincible = null;
            }

            if (underfootPlatform != null) {
                player.Position.Y = underfootPlatform.Top;
            }

            player.Add(new Coroutine(RestoreCameraLockMode(level, savedCameraLockMode, savedCameraPosition)));

            lookout.RemoveSelf();
        }

        private static IEnumerator RestoreCameraLockMode(Level level, Level.CameraLockModes cameraLockMode,
            Vector2 cameraPosition) {
            while (Vector2.Distance(level.Camera.Position, cameraPosition) > 1f) {
                yield return null;
            }

            level.CameraLockMode = cameraLockMode;
        }

        private class LookoutComponent : Component {
            public LookoutComponent() : base(false, false) { }
        }
    }
}