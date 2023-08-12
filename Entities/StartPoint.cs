using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DJMapHelper.Entities;

// Search for a spawn point near this entity when the chapter begins
[Obsolete("Please use Player (Spawn Point) instead of this")]
[CustomEntity("DJMapHelper/startPoint")]
public class StartPoint : Entity {
    public StartPoint(EntityData data, Vector2 offset) : base(data.Position + offset) {
        if (Engine.Scene is not LevelLoader levelLoader || levelLoader.Level.Session is not { } session) {
            return;
        }

        if (session.FirstLevel && session.StartedFromBeginning && session.JustStarted && !session.HitCheckpoint && session.LevelData.Spawns.Count > 0) {
            session.RespawnPoint = session.GetSpawnPoint(data.Position + offset);
        }
    }
}