using Microsoft.Xna.Framework;
using Leore.Objects.Items;
using SPG.Objects;
using SPG.Util;
using System;

namespace Leore.Objects.Effects.Emitters
{
    public class ObtainParticle : Particle
    {
        float lastX, lastY;

        public ObtainParticle(ParticleEmitter emitter, float radius, float initialSpeed) : base(emitter)
        {
            LifeTime = 120;

            Angle = RND.Int(360);

            var lx = (float)MathUtil.LengthDirX(Angle);
            var ly = (float)MathUtil.LengthDirY(Angle);

            Position = new Vector2(Emitter.X + lx * radius, Emitter.Y + ly * radius);

            Scale = new Vector2(0);
            Alpha = 0;

            XVel = lx * initialSpeed;
            YVel = ly * initialSpeed;

            lastX = Emitter.X;
            lastY = Emitter.Y;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            //var ang = (float)new Vector2(Position.X - Emitter.Position.X, Position.Y - Emitter.Position.Y).VectorToAngle();            
            //var lx = (float)MathUtil.LengthDirX(ang);
            //var ly = (float)MathUtil.LengthDirY(ang);
            //XVel -= lx * .06f;
            //YVel -= ly * .06f;

            XVel += (Emitter.X - Position.X) / 800f;
            YVel += (Emitter.Y - Position.Y) / 800f;

            //XVel = Math.Sign(XVel) * Math.Min(Math.Abs(XVel), 3);
            //YVel = Math.Sign(YVel) * Math.Min(Math.Abs(YVel), 3);

            var s = (float)Math.Min(Math.Max(MathUtil.Euclidean(Position, Emitter.Position) / (1f * Globals.TILE), 0), 3);            
            Scale = new Vector2(Math.Min(Scale.X + .1f, s));
            
            if (MathUtil.Euclidean(Position, Emitter.Position) < 8)
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

            Position += new Vector2(Emitter.X - lastX, Emitter.Y - lastY);

            lastX = Emitter.X;
            lastY = Emitter.Y;
        }
    }

    public class ObtainParticleEmitter : ParticleEmitter
    {
        float curTimeout;
        float radius;
        float initialSpeed;
        
        public ObtainParticleEmitter(float x, float y, float timeout = 30, float radius = 5 * 16, float initialSpeed = 1) : base(x, y)
        {
            SpawnTimeout = (int)curTimeout;
            curTimeout = timeout;
            this.radius = radius;
            this.initialSpeed = initialSpeed;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            curTimeout = Math.Max(curTimeout - .1f, 2);
            SpawnTimeout = (int)curTimeout;

            if (!Active && Particles.Count == 0)
            {
                Destroy();
            }
        }

        public override void CreateParticle()
        {
            var particle = new ObtainParticle(this, radius, initialSpeed);
            particle.Color = Color;
        }
    }
}
