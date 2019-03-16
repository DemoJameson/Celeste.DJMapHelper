using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;

namespace Celeste.Mod.DJMapHelper.DebugMode {
    // Ctrl+Q: Add tower viewer.
    public static class LookoutBuilder {
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

            if (keyboard.Pressed(Keys.Q) && (keyboard.Check(Keys.LeftControl) || keyboard.Check(Keys.RightControl))) {
                level.Add(new Lookout(new EntityData {Position = self.Position}, Vector2.Zero));
            }
        }
    }
}