using System.Collections;
using System.Reflection;
using Celeste.Mod.DJMapHelper.Extensions;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DJMapHelper.DebugFeatures {
    public static class LookoutBuilder {
        private static bool? SavedInvincible {
            get => DJMapHelperModule.Session.SavedInvincible;
            set => DJMapHelperModule.Session.SavedInvincible = value;
        }

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
                && self.SceneAs<Level>() is { } level
                && level.Tracker.GetEntity<PortableLookout>() != null) {
                return true;
            }

            return orig(self, downCheck);
        }

        private static void LevelOnUpdate(On.Celeste.Level.orig_Update orig, Level level) {
            orig(level);

            if (level.Tracker.GetEntity<PortableLookout>() == null && SavedInvincible != null) {
                SaveData.Instance.Assists.Invincible = (bool) SavedInvincible;
                SavedInvincible = null;
            }

            Player player = level.Tracker.GetEntity<Player>();
            if (player == null) {
                return;
            }

            if (level.Paused || level.Transitioning || level.SkippingCutscene || level.InCutscene || player.Dead ||
                player.StateMachine.State == Player.StDummy) {
                return;
            }

            if (DJMapHelperModule.Settings.SpawnTowerViewer.Pressed) {
                DJMapHelperModule.Settings.SpawnTowerViewer.ConsumePress();
                if (level.Tracker.GetEntity<PortableLookout>() != null) {
                    return;
                }

                PortableLookout lookout = new(new EntityData {
                    Position = player.Position,
                    Level = level.Session.LevelData,
                    Name = "towerviewer",
                    ID = 1234567890
                }, Vector2.Zero);
                lookout.Add(new Coroutine(Look(lookout)));
                level.Add(lookout);

                // 恢复 LookoutBlocker 后用普通望远镜可能会卡住，因为镜头没有完全还原例如 10 j-16，所以干脆不恢复 LookoutBlocker
                level.Remove(level.Tracker.GetEntitiesCopy<LookoutBlocker>());
            }
        }

        private static IEnumerator Look(PortableLookout lookout) {
            Player player = Engine.Scene.Tracker.GetEntity<Player>();
            if (player?.Scene == null || player.Dead) {
                yield break;
            }

            InteractMethod?.Invoke(lookout, new object[] {player});
            SavedInvincible = SaveData.Instance.Assists.Invincible;
            SaveData.Instance.Assists.Invincible = true;

            Level level = player.SceneAs<Level>();
            Level.CameraLockModes savedCameraLockMode = level.CameraLockMode;
            Vector2 savedCameraPosition = level.Camera.Position;
            level.CameraLockMode = Level.CameraLockModes.None;

            Entity underfootPlatform = player.CollideFirstOutside<FloatySpaceBlock>(player.Position + Vector2.UnitY);

            bool interacting = (bool) InteractingField.GetValue(lookout);
            while (!interacting) {
                player.Position = lookout.Position;
                interacting = (bool) InteractingField.GetValue(lookout);
                yield return null;
            }

            while (interacting) {
                player.Position = lookout.Position;
                interacting = (bool) InteractingField.GetValue(lookout);
                yield return null;
            }

            lookout.Collidable = lookout.Visible = false;

            if (SavedInvincible != null) {
                SaveData.Instance.Assists.Invincible = (bool) SavedInvincible;
                SavedInvincible = null;
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

        [Tracked]
        [TrackedAs(typeof(Lookout))]
        private class PortableLookout : Lookout {
            public PortableLookout(EntityData data, Vector2 offset) : base(data, offset) { }
        }
    }
}