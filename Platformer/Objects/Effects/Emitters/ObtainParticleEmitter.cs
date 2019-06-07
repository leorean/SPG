using Microsoft.Xna.Framework;
using Platformer.Objects.Items;
using SPG.Objects;
using SPG.Util;
using System;

namespace Platformer.Objects.Effects.Emitters
{
    public class ObtainParticle : Particle
    {
        public ObtainParticle(ParticleEmitter emitter) : base(emitter)
        {
            LifeTime = 90;

            Angle = (float)RND.Next * 360;

            var lx = (float)MathUtil.LengthDirX(Angle) * 5 * Globals.TILE;
            var ly = (float)MathUtil.LengthDirY(Angle) * 5 * Globals.TILE;

            Position = new Vector2(Emitter.Position.X + lx, Emitter.Position.Y + ly);

            Scale = new Vector2(3);
            Alpha = 0;

            XVel = ((float)MathUtil.LengthDirX(Angle)) * 1f;
            YVel = ((float)MathUtil.LengthDirY(Angle)) * 1f;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            var tx = Emitter.X;
            var ty = Emitter.Y;

            XVel += (tx - Position.X) / 800f;
            YVel += (ty - Position.Y) / 800f;

            XVel = Math.Sign(XVel) * Math.Min(Math.Abs(XVel), 3);
            YVel = Math.Sign(YVel) * Math.Min(Math.Abs(YVel), 3);

            var s = Math.Min(Math.Max(Math.Abs(tx - Position.X) / (1 * Globals.TILE), 0), 3);            
            Scale = new Vector2(s);
            
            if (Math.Abs(Position.X - Emitter.X) < 8)
            {
                var shineEmitter = (Emitter.Parent as AbilityItem)?.ObtainShineEmitter;

                if (shineEmitter != null)
                {
                    //shineEmitter.GlowAlpha = Math.Min(shineEmitter.GlowAlpha + .03f, .125f);
                    shineEmitter.GlowScale = Math.Min(shineEmitter.GlowScale + .1f, 1f);
                }

                LifeTime = 0;
            }

            Alpha = Math.Min(Alpha + .05f, .5f);
        }
    }

    public class ObtainParticleEmitter : ParticleEmitter
    {
        float curTimeout;

        public ObtainParticleEmitter(float x, float y, float timeout = 30) : base(x, y)
        {
            SpawnTimeout = (int)curTimeout;
            curTimeout = timeout;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            curTimeout = Math.Max(curTimeout - .1f, 2);
            SpawnTimeout = (int)curTimeout;
        }

        public override void CreateParticle()
        {
            var particle = new ObtainParticle(this);            
        }
    }
}
