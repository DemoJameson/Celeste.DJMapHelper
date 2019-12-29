using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Celeste.Mod.DJMapHelper;
using Celeste.Mod.DJMapHelper.Extensions;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DJMapHelper.Entities
{
    public class DreamBlockYellow : DreamBlock
    {
        private static readonly Color ActiveBackColor = Color.Yellow;
        private static readonly Color ActiveLineColor = Color.White;
        private static readonly FieldInfo ColorBackFieldInfo = typeof(DreamBlock).GetPrivateField("activeBackColor");
        private static readonly FieldInfo ColorLineFieldInfo = typeof(DreamBlock).GetPrivateField("activeLineColor");
        public DreamBlockYellow(EntityData data, Vector2 offset):base(data, offset)
        {
            ColorBackFieldInfo?.SetValue(this, ActiveBackColor);
            ColorLineFieldInfo?.SetValue(this, ActiveLineColor);
        }

        public static void OnLoad()
        {
            On.Celeste.Player.DreamDashBegin += YellowDreamDashBegin;
        }
        
        public static void OnUnLoad()
        {
            On.Celeste.Player.DreamDashBegin -= YellowDreamDashBegin;
        }

        private static void YellowDreamDashBegin(On.Celeste.Player.orig_DreamDashBegin orig, Player player)
        {
            FieldInfo DreamBlockFieldInfo = typeof(Player).GetPrivateField("dreamBlock");
            DreamBlock dreamBlock = (DreamBlock) DreamBlockFieldInfo?.GetValue(player);
            if(dreamBlock.GetType() == typeof(DreamBlockYellow))
            {
                Audio.Play("event:/game/general/spring", player.BottomCenter);
                bool origDashAttacking = player.DashAttacking;
                float origSpeedX = player.Speed.X;
                if (origDashAttacking && player.Speed == Vector2.Zero)
                {
                    Audio.Play("event:/game/general/strawberry_touch", player.Position);
                    FieldInfo beforeDashSpeedFieldInfo = typeof(Player).GetPrivateField("beforeDashSpeed");
                    Vector2 bDSpeed = (Vector2)beforeDashSpeedFieldInfo?.GetValue(player);
                    origSpeedX = bDSpeed.X;
                }
                Vector2 newSpeed = player.DashDir * 240f;
                if (Math.Sign(origSpeedX) == Math.Sign(newSpeed.X) && Math.Abs(origSpeedX) > (double) Math.Abs(newSpeed.X))
                    newSpeed.X = origSpeedX;
                orig(player);
                player.Speed.X = newSpeed.X;
            }
            else
            {
                Audio.Play("event:/game/general/key_get", player.Position);
                orig(player);
            }
        }
    }
}