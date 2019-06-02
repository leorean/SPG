using Microsoft.Xna.Framework;
using SPG.Objects;
using SPG.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects.Effects.Emitters
{
    public class SingleParticle : Particle
    {
        private float s;

        public SingleParticle(ParticleEmitter emitter) : base(emitter)
        {
            s = RND.Choose(1, 2, 3);

            Angle = (float)RND.Next * 360f;

            LifeTime = 60;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Scale = new Vector2(s * LifeTime / 60f);
            Alpha = (float)(LifeTime / 60f);

            Color = new Color(Color, Alpha);
        }
    }

    class SingleParticleEmitter : ParticleEmitter
    {
        public SingleParticleEmitter(float x, float y) : base(x, y)
        {
            SpawnRate = 1;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            SpawnRate = 0;
            if (Particles.Count == 0)
                Destroy();
        }

        public override void CreateParticle()
        {
            var part = new SingleParticle(this);
            part.Color = Color;
            part.XVel = XVel;
            part.YVel = YVel;
        }
    }
}
