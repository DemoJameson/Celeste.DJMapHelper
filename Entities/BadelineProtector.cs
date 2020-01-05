using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Celeste.Mod.DJMapHelper;
using Celeste.Mod.DJMapHelper.Extensions;
using Microsoft.Xna.Framework;
using Monocle;
namespace Celeste.Mod.DJMapHelper.Entities
{
    public class BadelineProtector:Entity
    {
        public BadelineDummy badeline;
        private Player player;
        private const float RespwanTime = 8f;
        private float respawnTimer;
        private float rotationPercent;
        private const float length = 24f;
        public float Angle
        {
            get
            {
                return MathHelper.Lerp(3.141593f, -3.141593f, Easer(rotationPercent));
            }
        }
        
        private float Easer(float v)
        {
            return v;
        }
        public BadelineProtector() : base()
        {
            respawnTimer = 0.0f;
            rotationPercent = 0.0f;
            badeline = new BadelineDummy(Vector2.Zero);
            badeline.Sprite.Play("fallSlow", false, false);
            Position = badeline.Position;
            Collider = new Hitbox(8f, 9f, -4f, -11f);
            badeline.Visible = false;
        }
        
        public override void Added(Scene scene) {
            base.Added(scene);
            player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if(player == null)
                RemoveSelf();
            Scene.Add(badeline);
            ResetPosition();
            Appear();
        }

        public void ResetPosition()
        {
            rotationPercent -= Engine.DeltaTime / 1.8f;
            ++rotationPercent;
            rotationPercent %= 1f;
            badeline.Position = player.Center + Calc.AngleToVector(Angle, length);
            badeline.Sprite.Scale.X = (float) Math.Sign(badeline.X - player.X);
            Position = badeline.Position;
        }
        
        public override void Update()
        {
            base.Update();
            ResetPosition();
            if ((double) respawnTimer > 0.0)
            {
                respawnTimer -= Engine.DeltaTime;
                if ((double) respawnTimer <= 0.0)
                {
                    Appear();
                }
            }

            if (badeline.Visible)
            {
                foreach (TouchSwitch entity in Scene.Tracker.GetEntities<TouchSwitch>())
                {
                    if (CollideCheck((Entity) entity) && !entity.Switch.Activated)
                    {
                        entity.TurnOn();
                        respawnTimer = RespwanTime;
                        Vanish();
                        break;
                    }
                }
            }
            if (badeline.Visible)
            {
                foreach (Seeker entity in Scene.Tracker.GetEntities<Seeker>())
                {
                    if (CollideCheck((Entity) entity))
                    {
                        MethodInfo SeekerGotBounced = typeof(Seeker).GetPrivateMethod("GotBouncedOn");
                        SeekerGotBounced.Invoke(entity, new object[1]{this});
                        respawnTimer = RespwanTime;
                        Vanish();
                        break;
                    }
                }
            }

            if (badeline.Visible)
            {
                foreach (FinalBossShot entity in Scene.Tracker.GetEntities<FinalBossShot>())
                {
                    if (CollideCheck(entity))
                    {
                        entity.RemoveSelf();
                        respawnTimer = RespwanTime;
                        Vanish();
                        break;
                    }
                }
            }
            
        }
        
        public void Vanish()
        {
            Audio.Play("event:/char/badeline/disappear", badeline.Position);
            SceneAs<Level>().Displacement.AddBurst(badeline.Center, 0.5f, 24f, 96f, 0.4f, (Ease.Easer) null, (Ease.Easer) null);
            SceneAs<Level>().Particles.Emit(BadelineOldsite.P_Vanish, 12, badeline.Center, Vector2.One * 6f);
            badeline.Visible = false;
        }
        
        public void Appear()
        {
            Audio.Play("event:/char/badeline/appear", badeline.Position);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            SceneAs<Level>().Displacement.AddBurst(badeline.Center, 0.5f, 24f, 96f, 0.4f, (Ease.Easer) null, (Ease.Easer) null);
            SceneAs<Level>().Particles.Emit(BadelineOldsite.P_Vanish, 12, badeline.Center, Vector2.One * 6f);
            badeline.Visible = true;
        }

    }
}