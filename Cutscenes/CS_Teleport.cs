using System;
using System.Collections;
using Celeste.Mod.DJMapHelper.Triggers;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DJMapHelper.Cutscenes {
    public class CS_Teleport : CutsceneEntity {
        private readonly Player player;
        private readonly bool sitFire;
        private readonly string teleportRoom;
        private readonly Vector2 spawnPoint;
        private Bonfire bonfire;
        private int maxDashes;
        private TeleportTrigger.Dreams dreams;

        public CS_Teleport(Player player, bool sitFire, string teleportRoom, Vector2 spawnPoint, TeleportTrigger.Dreams dreams)
            : base(false) {
            this.player = player;
            this.sitFire = sitFire;
            this.teleportRoom = teleportRoom;
            this.spawnPoint = spawnPoint;
            this.dreams = dreams;
        }

        public override void OnBegin(Level level) {
            maxDashes = level.Session.Inventory.Dashes;
            level.Session.Inventory.Dashes = 1;
            
            if (sitFire) {
                bonfire = Scene.Tracker.GetEntity<Bonfire>();
            }
            else {
                bonfire = null;
            }

            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level) {
            player.StateMachine.State = Player.StDummy;
            player.Dashes = 1;
            Audio.SetMusic(null);
            if (bonfire != null) {
                yield return 1.0f;
                yield return player.DummyWalkTo(bonfire.X - 12f);
                yield return 0.2f;
                player.Facing = Facings.Right;
                player.DummyAutoAnimate = false;
                player.Sprite.Play("duck");
                yield return 0.5f;
                bool dreaming = level.Session.Dreaming;
                level.Session.Dreaming = false;
                bonfire.SetMode(Bonfire.Mode.Lit);
                level.Session.Dreaming = dreaming;
                yield return 1f;
                player.Sprite.Play("idle");
                yield return 0.4f;
                player.DummyAutoAnimate = true;
                yield return player.DummyWalkTo(bonfire.X - 24f);
                yield return 0.4f;
                player.DummyAutoAnimate = false;
                player.Facing = Facings.Right;
            }
            else {
                player.Facing = Facings.Right;
                yield return 1.0f;
                player.DummyAutoAnimate = false;
            }

            player.Sprite.Play("sleep");
            Audio.Play("event:/char/madeline/campfire_sit", player.Position);
            yield return 3f;
            FadeWipe fadeWipe = new FadeWipe(level, false, () => EndCutscene(level));
            level.Add(fadeWipe);
        }

        public override void OnEnd(Level level) {
            level.OnEndOfFrame += (Action) (() => {
                level.Remove(player);
                level.UnloadLevel();
                switch (dreams)
                {
                    case TeleportTrigger.Dreams.Awake:
                        level.Session.Dreaming = false;
                        break;
                    case TeleportTrigger.Dreams.Dreaming:
                        level.Session.Dreaming = true;
                        break;
                }  
                level.Session.Level = teleportRoom;
                level.Session.RespawnPoint = level.GetSpawnPoint(new Vector2(level.Bounds.Left, level.Bounds.Top) + spawnPoint);
                level.Session.Inventory.Dashes = maxDashes;
                level.LoadLevel(Player.IntroTypes.WakeUp);
            });
        }
    }
}