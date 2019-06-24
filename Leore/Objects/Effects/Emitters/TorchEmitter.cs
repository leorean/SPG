using Leore.Resources;
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

    public class TorchParticle : Particle
    {
        private int maxLifeTime = 90;

        public TorchParticle(ParticleEmitter emitter) : base(emitter)
        {
            int colorIndex = RND.Int(GameResources.FireColors.Count - 1);
            Color = GameResources.FireColors[colorIndex];
            
            Alpha = 1f;

            Angle = (float) (RND.Next * 2 * Math.PI);

            LifeTime = maxLifeTime;

            YVel = -.05f - (float)RND.Next * .5f;

        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var r = LifeTime / (float)maxLifeTime;

            Scale = new Vector2(r) * 2;
            Alpha = r * .5f;
        }
    }

    public class TorchEmitter : ParticleEmitter
    {
        public TorchEmitter(float x, float y) : base(x, y)
        {
            SpawnRate = 1;
            SpawnTimeout = 2;
        }

        public override void CreateParticle()
        {
            new TorchParticle(this) { Position = this.Position + new Vector2(-4 + RND.Int(8), -4 + RND.Int(8)) };
        }

        private Vector2 Vector2()
        {
            throw new NotImplementedException();
        }
    }
}
