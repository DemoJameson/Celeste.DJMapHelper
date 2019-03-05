using System;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.DJMapHelper.Triggers {
    public class MaxDashesTrigger : Trigger {
        private enum DashesNum {
            One,
            Two
        }

        private readonly DashesNum dashesNum;

        public MaxDashesTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            dashesNum = data.Enum("dashes", DashesNum.One);
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);
            switch (dashesNum) {
                case DashesNum.One:
                    SceneAs<Level>().Session.Inventory.Dashes = 1;
                    if (player.Dashes > 1) {
                        player.Dashes = 1;
                    }

                    break;
                case DashesNum.Two:
                    SceneAs<Level>().Session.Inventory.Dashes = 2;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}