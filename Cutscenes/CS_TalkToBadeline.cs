using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DJMapHelper.Cutscenes {
    public class CS_TalkToBadeline : CutsceneEntity {
        private readonly string dialogEntry;
        private readonly bool endLevel;
        private readonly Player player;
        private readonly bool rejoin;
        private BadelineDummy badeline;
        private int maxDashes;

        public CS_TalkToBadeline(Player player, string dialogEntry, bool endLevel, bool rejoin)
            : base(false) {
            this.player = player;
            this.dialogEntry = dialogEntry;
            this.endLevel = endLevel;
            this.rejoin = rejoin;
        }

        public override void OnBegin(Level level) {
            maxDashes = level.Session.Inventory.Dashes;
            level.Session.Inventory.Dashes = 1;

            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level) {
            player.StateMachine.State = Player.StDummy;
            yield return 0.5f;
            yield return BadelineAppears();
            yield return 0.3f;
            yield return Textbox.Say(dialogEntry);
            if (badeline != null && !rejoin) {
                yield return BadelineVanishes();
            }
            else if (badeline != null) {
                yield return BadelineRejoin();
            }

            EndCutscene(level);
        }

        public override void OnEnd(Level level) {
            level.OnEndOfFrame += (Action) (() => {
                level.Session.Inventory.Dashes = maxDashes;
                player.Dashes = maxDashes;
                player.Depth = 0;
                player.Active = true;
                player.Visible = true;
                player.StateMachine.State = Player.StNormal;
                badeline?.RemoveSelf();
                if (endLevel) {
                    level.CompleteArea(true, false, false);
                    player.StateMachine.State = Player.StDummy;
                    SpotlightWipe.FocusPoint += new Vector2(0.0f, 0f);
                }
            });
        }

        private IEnumerator BadelineAppears() {
            Audio.Play("event:/char/badeline/maddy_split", player.Position);
            Level.Add(badeline = new BadelineDummy(player.Center));
            Level.Displacement.AddBurst(badeline.Center, 0.5f, 8f, 32f, 0.5f);
            Level.Session.Inventory.Dashes = 1;
            player.Dashes = 1;
            badeline.Sprite.Scale.X = -1f;
            yield return badeline.FloatTo(player.Center + new Vector2(18f, -10f), -1, false);
            yield return 0.2f;
            player.Facing = Facings.Right;
            yield return null;
        }

        private IEnumerator BadelineRejoin() {
            Audio.Play("event:/new_content/char/badeline/maddy_join_quick", badeline.Position);
            Vector2 from = badeline.Position;
            for (var p = 0.0f; (double) p < 1.0; p += Engine.DeltaTime / 0.25f) {
                badeline.Position = Vector2.Lerp(from, player.Position, Ease.CubeIn(p));
                yield return null;
            }

            Level.Displacement.AddBurst(player.Center, 0.5f, 8f, 32f, 0.5f);
            badeline.RemoveSelf();
        }

        private IEnumerator BadelineVanishes() {
            yield return 0.2f;
            badeline.Vanish();
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            badeline = null;
            yield return 0.2f;
        }
    }
}