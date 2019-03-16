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

        public static void OnLoad() {
            On.Celeste.Player.Update += PlayerOnUpdate;
        }

        public static void OnUnload() {
            On.Celeste.Player.Update -= PlayerOnUpdate;
        }

        private static void PlayerOnUpdate(On.Celeste.Player.orig_Update orig, Player self) {
            orig(self);

            Level level = self.SceneAs<Level>();

            if (self.Dead || level.Paused) {
                return;
            }

            MInput.KeyboardData keyboard = MInput.Keyboard;

            if (keyboard.Pressed(Keys.Q) && (keyboard.Check(Keys.LeftControl) || keyboard.Check(Keys.RightControl)) && self.StateMachine.State != Player.StDummy) {
                Lookout lookout = new Lookout(new EntityData {Position = self.Position}, Vector2.Zero);
                lookout.Add(new Coroutine(Look(lookout, self)));
                level.Add(lookout);
            }
        }

        private static IEnumerator Look(Lookout lookout, Player player) {
            yield return null;
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
            lookout.RemoveSelf();
        }
    }
}