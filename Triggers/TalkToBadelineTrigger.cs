using Celeste.Mod.DJMapHelper.Cutscenes;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.DJMapHelper.Triggers {
    public class TalkToBadelineTrigger : Trigger {
        private readonly string dialogEntry;
        private readonly bool endLevel;
        private readonly bool rejoin;

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

            Session session = player.SceneAs<Level>().Session;
            if (session.GetFlag(dialogEntry)) return;
            session.SetFlag(dialogEntry);
            player.Scene.Add(new CS_TalkToBadeline(player, dialogEntry, endLevel, rejoin));
        }
    }
}