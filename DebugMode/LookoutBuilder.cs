using System.Collections;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;

namespace Celeste.Mod.DJMapHelper.DebugMode {
    // Ctrl+Q: Add tower viewer.
    public static class LookoutBuilder {
        private static readonly MethodInfo InteractMethod = typeof(Lookout).GetMethod("Interact", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo InteractingField = typeof(Lookout).GetPrivateField("interacting");
        private static bool AlwaysOnGround;
        public static void OnLoad() {
            On.Celeste.Player.Update += PlayerOnUpdate;
            On.Celeste.Actor.OnGround_int += ActorOnOnGroundInt;
        }

        public static void OnUnload() {
            On.Celeste.Player.Update -= PlayerOnUpdate;
            On.Celeste.Actor.OnGround_int -= ActorOnOnGroundInt;
        }

        private static bool ActorOnOnGroundInt(On.Celeste.Actor.orig_OnGround_int orig, Actor self, int downCheck) {
            if (AlwaysOnGround && self is Player && DJMapHelperModule.Settings.EnableTowerViewer) {
                return true;
            }
            return orig(self, downCheck);
        }

        private static void PlayerOnUpdate(On.Celeste.Player.orig_Update orig, Player self) {
            orig(self);

            if (!DJMapHelperModule.Settings.EnableTowerViewer) {
                return;
            }

            Level level = self.SceneAs<Level>();

            if (self.Dead || level.Paused || self.StateMachine.State == Player.StDummy) {
                return;
            }

            MInput.KeyboardData keyboard = MInput.Keyboard;

            if (keyboard.Pressed(Keys.Q) && (keyboard.Check(Keys.LeftControl) || keyboard.Check(Keys.RightControl))) {
                Lookout lookout = new Lookout(new EntityData {Position = self.Position}, Vector2.Zero);
                lookout.Add(new Coroutine(Look(lookout, self)));
                level.Add(lookout);
            }
        }

        private static IEnumerator Look(Lookout lookout, Player player) {
            yield return null;
            AlwaysOnGround = true;
            InteractMethod?.Invoke(lookout, new object[] {player});
            bool invincible = SaveData.Instance.Assists.Invincible;
            SaveData.Instance.Assists.Invincible = true;
            
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

            SaveData.Instance.Assists.Invincible = invincible;
            AlwaysOnGround = false;
            lookout.RemoveSelf();
        }
    }
}