using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DJMapHelper.Entities; 

// Search for a spawn point near this entity when the chapter begins
[CustomEntity("DJMapHelper/startPoint")]
public class StartPoint : Entity {
    public StartPoint(EntityData data, Vector2 offset) : base(data.Position + offset) {
        if (!(Engine.Scene is LevelLoader levelLoader) || !(levelLoader.Level.Session is Session session)) {
            return;
        }

        if (session.FirstLevel && session.StartedFromBeginning && session.JustStarted) {
            session.RespawnPoint = session.GetSpawnPoint(data.Position + offset);
        }
    }
}