using Microsoft.Xna.Framework;
using Celeste.Mod.DJMapHelper.Cutscenes;
using Monocle;

namespace Celeste.Mod.DJMapHelper.Triggers {
    public class TalkToBadelineTrigger : Trigger {
        private string dialogEntry;
        private bool endLevel;
        private bool rejoin;

        public TalkToBadelineTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            dialogEntry = data.Attr("dialogId");
            endLevel = data.Bool(nameof(endLevel));
            rejoin = data.Bool(nameof(rejoin));
        }


        public override void OnEnter(Player player) {
            base.OnEnter(player);
            RemoveSelf();

            if (string.IsNullOrEmpty(dialogEntry)) {
                player.Scene.Add(new MiniTextbox("DJ_MAP_HELPER_TALK_TO_BADELINE_TRIGGER_DIALOG_ID_EMPTY"));
                return;
            }

            player.Scene.Add(new CS_TalkToBadeline(player, dialogEntry, endLevel, rejoin));
        }
    }
}