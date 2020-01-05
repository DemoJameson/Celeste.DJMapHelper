using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DJMapHelper.Cutscenes {
    public class CS_PayphoneCall : CutsceneEntity {
        private readonly bool answer;
        private readonly string dialogEntry;
        private readonly bool endLevel;
        private readonly Player player;
        private Payphone payphone;
        private SoundSource phoneSfx;
        private SoundSource ringtone;
        private Sprite sprite;

        public CS_PayphoneCall(Player player, string dialogEntry, bool endLevel, bool answer)
            : base(false) {
            this.player = player;
            this.dialogEntry = dialogEntry;
            this.endLevel = endLevel;
            this.answer = answer;
        }

        public override void OnBegin(Level level) {
            payphone = Scene.Tracker.GetEntity<Payphone>();
            Add(new Coroutine(Cutscene(level)));
            Add(ringtone = new SoundSource());
            Add(phoneSfx = new SoundSource());
            Add(sprite = new Sprite(GFX.Game, "cutscenes/payphone/phone"));
            sprite.Add("putdown", "", 0.08f, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1);
            sprite.Justify = new Vector2(0.5f, 1f);
            sprite.Visible = false;
        }

        private IEnumerator Cutscene(Level level) {
            if (answer) {
                ringtone.Position = payphone.Position;
                player.StateMachine.State = 11;
                player.Dashes = 1;
                yield return 0.3f;
                ringtone.Play("event:/game/02_old_site/sequence_phone_ring_loop", null, 0.0f);
                yield return 1.2f;
                if (player.X < payphone.X - 24f || player.X > payphone.X - 4f)
                    yield return player.DummyWalkTo(payphone.X - 24f, false, 1f, false);
                yield return 1.5f;
                player.Facing = Facings.Left;
                yield return 1.5f;
                player.Facing = Facings.Right;
                yield return 0.25f;
                yield return player.DummyWalkTo(payphone.X - 4f, false, 1f, false);
                yield return 1.5f;
                Add(Alarm.Create(Alarm.AlarmMode.Oneshot, () => ringtone.Param("end", 1f), 0.43f, true));
                player.Visible = false;
                Audio.Play("event:/game/02_old_site/sequence_phone_pickup", player.Position);
                yield return payphone.Sprite.PlayRoutine("pickUp", false);
                yield return 1f;
            }
            else {
                player.StateMachine.State = 11;
                player.Dashes = 1;
                yield return 0.3f;
                if (player.X < payphone.X - 24f || player.X > payphone.X - 4f)
                    yield return player.DummyWalkTo(payphone.X - 24f, false, 1f, false);
                yield return 1.5f;
                player.Facing = Facings.Left;
                yield return 1.5f;
                player.Facing = Facings.Right;
                yield return 0.25f;
                yield return player.DummyWalkTo(payphone.X - 4f, false, 1f, false);
                yield return 0.2f;
                yield return 0.5f;
                player.Visible = false;
                Audio.Play("event:/game/02_old_site/sequence_phone_pickup", player.Position);
                yield return payphone.Sprite.PlayRoutine("pickUp", false);
                yield return 0.25f;
                phoneSfx.Position = player.Position;
                phoneSfx.Play("event:/game/02_old_site/sequence_phone_ringtone_loop", null, 0.0f);
                yield return 6f;
                phoneSfx.Stop(true);
            }

            payphone.Sprite.Play("talkPhone", false, false);
            yield return Textbox.Say(dialogEntry);
            yield return 0.3f;
            payphone.Sprite.Visible = false;
            sprite.Visible = true;
            Audio.Play("event:/game/02_old_site/sequence_phone_pickup", player.Position);
            Position = payphone.Position;
            yield return sprite.PlayRoutine("putdown", false);
            EndCutscene(level);
        }

        public override void OnEnd(Level level) {
            level.OnEndOfFrame += (Action) (() => {
                player.Depth = 0;
                player.Speed = Vector2.Zero;
                player.Position.X = payphone.Position.X - 4f;
                player.Facing = Facings.Right;
                player.MoveVExact(100);
                sprite.Visible = false;
                payphone.Sprite.Visible = true;
                payphone.Sprite.Play("idle", false, false);
                player.Active = true;
                player.Visible = true;
                player.StateMachine.State = Player.StNormal;
                if (endLevel) {
                    level.CompleteArea(true, false, false);
                    player.StateMachine.State = Player.StDummy;
                    SpotlightWipe.FocusPoint += new Vector2(0.0f, 0f);
                }
            });
        }
    }
}