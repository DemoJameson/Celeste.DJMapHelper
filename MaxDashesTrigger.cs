using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.DJMapHelper
{
    public class MaxDashesTrigger : Trigger
    {
        public enum DashesNum
        {
            One,
            Two
        }

        public readonly DashesNum dashesNum;

        public MaxDashesTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            dashesNum = data.Enum("dashes", DashesNum.One);
        }

        public static void OnLoad()
        {
            On.Celeste.Trigger.OnEnter += PlayerEnterMaxDashesTrigger;
        }

        public static void OnUnload()
        {
            On.Celeste.Trigger.OnEnter -= PlayerEnterMaxDashesTrigger;
        }

        private static void PlayerEnterMaxDashesTrigger(On.Celeste.Trigger.orig_OnEnter orig, Trigger self,
            Player player)
        {
            if (self.GetType() == typeof(MaxDashesTrigger))
            {
                switch (((MaxDashesTrigger) self).dashesNum)
                {
                    case DashesNum.One:
                        self.SceneAs<Level>().Session.Inventory.Dashes = 1;
                        if (player.Dashes > 1)
                        {
                            player.Dashes = 1;
                        }

                        return;
                    case DashesNum.Two:
                        self.SceneAs<Level>().Session.Inventory.Dashes = 2;
                        return;
                }
            }
            orig(self, player);
        }
    }
}