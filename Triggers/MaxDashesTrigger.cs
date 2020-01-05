using System;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.DJMapHelper.Triggers {
    public class MaxDashesTrigger : Trigger {
        private static bool? NoRefills;

        private readonly DashesNum dashesNum;

        public MaxDashesTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            dashesNum = data.Enum("dashes", DashesNum.One);
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);
            Session session = SceneAs<Level>().Session;
            switch (dashesNum) {
                case DashesNum.Zero:
                    session.Inventory.Dashes = 0;

                    if (NoRefills == null) NoRefills = session.Inventory.NoRefills;

                    session.Inventory.NoRefills = true;
                    player.Dashes = 0;
                    break;
                case DashesNum.One:
                    session.Inventory.Dashes = 1;
                    if (player.Dashes > 1) player.Dashes = 1;

                    if (NoRefills != null) {
                        session.Inventory.NoRefills = (bool) NoRefills;
                        NoRefills = null;
                    }

                    break;
                case DashesNum.Two:
                    session.Inventory.Dashes = 2;

                    if (NoRefills != null) {
                        session.Inventory.NoRefills = (bool) NoRefills;
                        NoRefills = null;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private enum DashesNum {
            Zero,
            One,
            Two
        }
    }
}