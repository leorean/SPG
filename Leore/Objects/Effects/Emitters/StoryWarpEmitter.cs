using Leore.Main;
using Leore.Objects.Level;
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
            var s = 1 + (float)RND.Next * 1;
            Scale = new Vector2(s);

            Angle = (float)(RND.Next * 2 * Math.PI);

            Position = new Vector2(emitter.X - 8 + (float)RND.Next * 16, emitter.Y - s);
            YVel = -.25f - (float)RND.Next * .5f;

            LifeTime = 60;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Alpha = Math.Max(Alpha - .02f, 0);
            if (Alpha == 0)
            {
                LifeTime = 0;
            }

            Scale = new Vector2(Math.Max(Scale.X - .01f, .5f));
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
