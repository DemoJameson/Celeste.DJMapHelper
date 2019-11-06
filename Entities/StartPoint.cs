using System.Reflection;
using Celeste.Mod.DJMapHelper.Extensions;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DJMapHelper.Entities {
    // Search for a spawn point near this entity when the chapter begins
    public class StartPoint:Entity {
        private static readonly FieldInfo SessionField = typeof(LevelLoader).GetPrivateField("session");
        public StartPoint(EntityData data, Vector2 offset):base(data.Position + offset) {
            if (!(Engine.Scene is LevelLoader levelLoader) || !(SessionField.GetValue(levelLoader) is Session session)) {
                return;
            }

            if (session.FirstLevel && session.StartedFromBeginning && session.JustStarted) {
                session.RespawnPoint = session.GetSpawnPoint(data.Position + offset);
            }
        }  
    }
}