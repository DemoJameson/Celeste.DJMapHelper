using Microsoft.Xna.Framework;

namespace Celeste.Mod.DJMapHelper {
    public class DJMapHelperSession : EverestModuleSession {
        public bool? LastNoRefills { get; set; } = null;
        public Color? LastOverrideHairColor { get; set; } = null;
    }
}