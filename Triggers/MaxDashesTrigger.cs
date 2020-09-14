using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.DJMapHelper.Triggers {
    [CustomEntity("DJMapHelper/maxDashesTrigger")]
    public class MaxDashesTrigger : Trigger {
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

                    if (DJMapHelperModule.Session.LastNoRefills == null) {
                        DJMapHelperModule.Session.LastNoRefills = session.Inventory.NoRefills;
                        DJMapHelperModule.Session.LastOverrideHairColor = player.OverrideHairColor;
                        player.OverrideHairColor = player.Sprite.Mode == PlayerSpriteMode.MadelineAsBadeline ? Player.UsedBadelineHairColor : Player.UsedHairColor;
                    }

                    session.Inventory.NoRefills = true;
                    player.Dashes = 0;
                    break;
                case DashesNum.One:
                    session.Inventory.Dashes = 1;
                    if (player.Dashes > 1) {
                        player.Dashes = 1;
                    }

                    if (DJMapHelperModule.Session.LastNoRefills != null) {
                        session.Inventory.NoRefills = (bool) DJMapHelperModule.Session.LastNoRefills;
                        player.OverrideHairColor = DJMapHelperModule.Session.LastOverrideHairColor;
                        DJMapHelperModule.Session.LastNoRefills = null;
                        DJMapHelperModule.Session.LastOverrideHairColor = null;
                    }

                    break;
                case DashesNum.Two:
                    session.Inventory.Dashes = 2;

                    if (DJMapHelperModule.Session.LastNoRefills != null) {
                        session.Inventory.NoRefills = (bool) DJMapHelperModule.Session.LastNoRefills;
                        player.OverrideHairColor = DJMapHelperModule.Session.LastOverrideHairColor;
                        DJMapHelperModule.Session.LastNoRefills = null;
                        DJMapHelperModule.Session.LastOverrideHairColor = null;
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