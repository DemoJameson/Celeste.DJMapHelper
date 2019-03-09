using System;
using Celeste.Mod.DJMapHelper.Cutscenes;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DJMapHelper.Triggers
{
    [Tracked]
    public class TeleportTrigger : Trigger
    {
        private readonly string room;
        private readonly bool bonfire;
        private readonly int spawnPointX;
        private readonly int spawnPointY;
        private bool triggered;

        public TeleportTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            bonfire = data.Bool("bonfire");
            room = data.Attr("room");
            spawnPointX = data.Int("spawnPointX");
            spawnPointY = data.Int("spawnPointY");

            if (string.IsNullOrEmpty(room)) {
                throw new ArgumentException("The room name must not be empty");
            }

            if (Engine.Scene is Level level && level.Session.MapData.Get(room) == null){
                throw new ArgumentException("The room name does not exist");
            }
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            
            if (triggered) {
                return;
            }
            
            triggered = true;
            Scene.Add(new CS_Teleport(player, bonfire, room, new Vector2(spawnPointX, spawnPointY)));
        }
    }
}