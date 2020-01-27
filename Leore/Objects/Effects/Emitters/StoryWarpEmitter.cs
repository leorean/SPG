using Microsoft.Xna.Framework;
using SPG.Objects;
using SPG.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leore.Objects.Effects.Emitters
{
    public class StoryWarpParticle : Particle
    {
        public StoryWarpParticle(ParticleEmitter emitter) : base(emitter)
        {
            Position = new Vector2(emitter.X - 8 + (float)RND.Next * 16, emitter.Y);
            YVel = -.5f - (float)RND.Next * 1;

            LifeTime = 60;

            var s =  1 + (float)RND.Next * 1;

            Scale = new Vector2(s);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Alpha = LifeTime / 60f;

            Scale = new Vector2(Math.Max(Scale.X - .02f, .5f));
        }
    }

    public class StoryWarpEmitter : ParticleEmitter
    {
        public StoryWarpEmitter(float x, float y) : base(x, y)
        {
        }

        public override void CreateParticle()
        {
            new StoryWarpParticle(this);
        }
    }
}
