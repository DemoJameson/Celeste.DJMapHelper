using Microsoft.Xna.Framework.Input;

namespace Celeste.Mod.DJMapHelper {
    public class DJMapHelperSettings : EverestModuleSettings {
        [SettingName("DJ_MAP_HELPER_ENABLE_ADD_TOWER_VIEWER")]
        [DefaultButtonBinding(0, Keys.None)]
        public ButtonBinding SpawnTowerViewer { get; set; } = new ButtonBinding();
    }
}