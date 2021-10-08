using Celeste.Mod.DJMapHelper.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.DJMapHelper {
    public class DJMapHelperSession : EverestModuleSession {
        public bool? LastNoRefills { get; set; } = null;
        public Color? LastOverrideHairColor { get; set; } = null;
        public BadelineProtectorConfig BadelineProtectorConfig { get; set; }
        public bool DefeatedBoss { get; set; } = false;
        public bool? SavedInvincible { get; set; } = null;
    }
}