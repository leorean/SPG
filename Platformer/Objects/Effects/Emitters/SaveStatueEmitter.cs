using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Objects;
using SPG.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects.Effects.Emitters
{
    public class SaveStatueParticle : Particle
    {
        public SaveStatueParticle(ParticleEmitter emitter) : base(emitter)
        {
            var posX = emitter.X - 4 + RND.Next * 8;
            var posY = emitter.Y + 3;

            LifeTime = 60;
            
            Position = new Vector2((float)posX, (float)posY);

            YVel = (float)(-.2 - RND.Next * .2);
            Scale = new Vector2(3, 3);
            Alpha = 0;

            Angle = (float)(RND.Next * 360);            
        }
        
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var s = Math.Max(Scale.X - .025f, 1);

            Scale = new Vector2(s);

            var relativeLifeTime = LifeTime / 60f;

            if (relativeLifeTime > .5f)
                Alpha = Math.Min(Alpha + .1f, 1);
            else
            {
                Alpha = Math.Max(Alpha - .05f, 0);
            }
        }
    }

    public class SaveStatueEmitter : ParticleEmitter
    {
        List<Color> particleColors;

        public SaveStatueEmitter(float x, float y) : base(x, y)
        {
            SpawnRate = .1f;

            particleColors = new List<Color>
            {
                new Color(255, 255, 255),
                new Color(206, 255, 255),
                new Color(168, 248, 248),
                new Color(104, 216, 248)
            };            
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void CreateParticle()
        {
            var particle = new SaveStatueParticle(this);

            var colorIndex = RND.Int(particleColors.Count - 1);
            particle.Color = particleColors[colorIndex];
        }
    }
}
