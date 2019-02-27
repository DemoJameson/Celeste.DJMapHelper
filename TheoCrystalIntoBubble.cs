using System.Linq;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.DJMapHelper {
    public static class TheoCrystalIntoBubble {
        private static bool allowTheoCrystalIntoBubble;

        public static void OnLoad() {
            On.Celeste.LevelLoader.ctor += LevelLoaderOnCtor;
            On.Celeste.Player.BoostBegin += PlayerOnBoostBegin;
        }

        public static void OnUnload() {
            On.Celeste.LevelLoader.ctor -= LevelLoaderOnCtor;
            On.Celeste.Player.BoostBegin -= PlayerOnBoostBegin;
        }

        private static void LevelLoaderOnCtor(On.Celeste.LevelLoader.orig_ctor orig, LevelLoader self, Session session,
            Vector2? startPosition) {
            orig(self, session, startPosition);
            allowTheoCrystalIntoBubble = session.MapData.Levels.Select(data => data.Name)
                .Any(levelName => levelName.Contains(nameof(allowTheoCrystalIntoBubble)));
        }

        private static void PlayerOnBoostBegin(On.Celeste.Player.orig_BoostBegin orig, Player self) {
            if (allowTheoCrystalIntoBubble) {
                self.RefillDash();
                self.RefillStamina();
            }
            else {
                orig(self);
            }
        }
    }
}