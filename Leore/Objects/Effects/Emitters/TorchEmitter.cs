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
            Depth = emitter.Depth + .0001f;
            int colorIndex = RND.Int(GameResources.FireColors.Count - 1);
            Color = GameResources.FireColors[colorIndex];
            
            Alpha = 0f;

            Angle = (float) (RND.Next * 2 * Math.PI);

            LifeTime = maxLifeTime;

            YVel = -.05f - (float)RND.Next * .3f;

        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            var r = LifeTime / (float)maxLifeTime;

            Scale = new Vector2(r) * 2;
            Alpha = (float)Math.Sin(r * Math.PI);
        }
    }

    public class TorchEmitter : ParticleEmitter
    {
        public int XRange { get; set; } = 6;
        public int YRange { get; set; } = 8;

        public TorchEmitter(float x, float y) : base(x, y)
        {
            SpawnRate = 1;
            SpawnTimeout = 4;
        }

        public override void CreateParticle()
        {
            new TorchParticle(this) { Position = this.Position + new Vector2(-.5f * XRange + RND.Int(XRange), -.5f * YRange + RND.Int(YRange)), Depth = Depth };
        }        
    }
}
