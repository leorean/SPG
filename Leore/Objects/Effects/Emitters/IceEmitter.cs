using Leore.Main;
using Leore.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Objects;
using SPG.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leore.Objects.Effects.Emitters
{
    public class IceParticle : Particle
    {
        public IceParticle(ParticleEmitter emitter) : base(emitter)
        {
            DrawOffset = new Vector2(8);
            LifeTime = 60;
            
            Texture = AssetManager.Particles[17 + RND.Int(3)];
            
            Depth = emitter.Depth;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            //frame = ((frame + fSpd) % (maxFrame - minFrame));
            //frame = Math.Min(frame + fSpd, maxFrame - 1);

            //Texture = AssetManager.Particles[(int)frame];

            var t = (LifeTime / 60f);
            Alpha = (float)Math.Sin(t * Math.PI) * .5f;

            Scale = new Vector2(.5f + (LifeTime / 60f) * .5f);

            YVel = Math.Min(YVel + .01f, 1);
            
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);
        }
    }

    public class IceEmitter : ParticleEmitter
    {
        public Vector2 SpawnPosition;

        public IceEmitter(float x, float y) : base(x, y)
        {
            SpawnTimeout = 3;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Particles.Count == 0)
                Destroy();
        }

        public override void CreateParticle()
        {
            var part = new IceParticle(this);
            part.Position = SpawnPosition + new Vector2(-4 + RND.Int(8), -4 + RND.Int(8));
        }
    }
}
