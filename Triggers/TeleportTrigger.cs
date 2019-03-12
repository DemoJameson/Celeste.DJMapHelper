using Celeste.Mod.DJMapHelper.Cutscenes;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DJMapHelper.Triggers {
    [Tracked]
    public class TeleportTrigger : Trigger {
        private readonly string room;
        private readonly bool bonfire;
        private readonly int spawnPointX;
        private readonly int spawnPointY;
        private bool triggered;
        private bool keepKey;

        public enum Dreams {
            Dreaming,
            Awake,
            NoChange
        }

        private readonly Dreams dreams;

        public TeleportTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            bonfire = data.Bool("bonfire");
            room = data.Attr("room");
            spawnPointX = data.Int("spawnPointX");
            spawnPointY = data.Int("spawnPointY");
            dreams = data.Enum("Dreaming", Dreams.NoChange);
            keepKey = data.Bool("KeepKey", false);
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            if (triggered) {
                return;
            }

            triggered = true;

            if (Engine.Scene is Level level) {
                if (string.IsNullOrEmpty(room)) {
                    level.Add(new MiniTextbox("DJ_MAP_HELPER_TELEPORT_TRIGGER_NAME_EMPTY"));
                    return;
                }

                if (level.Session.MapData.Get(room) == null) {
                    level.Add(new MiniTextbox("DJ_MAP_HELPER_TELEPORT_TRIGGER_NAME_NOT_EXIST"));
                    return;
                }
            }

            Scene.Add(new CS_Teleport(player, bonfire, room, new Vector2(spawnPointX, spawnPointY), dreams, keepKey));
        }
    }
}