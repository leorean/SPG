using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leore.Main;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Objects;
using SPG.Util;

namespace Leore.Objects.Effects.Emitters
{
    public class FireFlyParticle : Particle
    {
        private double t;
        private float maxLifeTime;
        Vector2 orig;
        double dist;
        int animDelay = 0;

        public FireFlyParticle(ParticleEmitter emitter) : base(emitter)
        {
            dist = RND.Choose(-1, 1) * (2 + RND.Int(8));
            maxLifeTime = 180 + RND.Int(180);
            LifeTime = (int)(maxLifeTime);

            orig = Position;

            Scale = new Vector2(.25f + .25f * (float)RND.Next);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var l = LifeTime / maxLifeTime;

            t = l * 2 * Math.PI;

            Alpha = (float)Math.Sin(l * Math.PI);

            var x = dist * Math.Cos(t);
            var y = dist * Math.Sin(t) * Math.Cos(t);
            
            Position = orig + new Vector2((float)x, (float)y);

            animDelay = Math.Max(animDelay - 1, 0);
            if (animDelay == 0)
            {
                Scale = new Vector2(Scale.X, -Scale.Y);
                animDelay = 2;
            }
        }

        //public override void Draw(SpriteBatch sb, GameTime gameTime)
        //{
        //    base.Draw(sb, gameTime);

        //    sb.Draw(AssetManager.LittleStuff[1], Position, null, new Color(Color, Alpha), Angle, DrawOffset, Scale, SpriteEffects.None, Depth + .0001f);
        //}
    }

    public class FireFlyEmitter : ParticleEmitter
    {
        public FireFlyEmitter(float x, float y) : base(x, y)
        {
            Texture = AssetManager.LittleStuff[0];
            DrawOffset = new Vector2(8);            
        }

        public override void CreateParticle()
        {
            if (Particles.Count == 0)
            {
                if (RND.Next * 100 > 2)
                    return;

                new FireFlyParticle(this)
                {
                    Texture = Texture,
                    DrawOffset = DrawOffset
                };
            }
        }
    }    
}
