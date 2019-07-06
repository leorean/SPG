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
        private float initialScale = 1;

        public TorchParticle(ParticleEmitter emitter) : base(emitter)
        {
            this.initialScale = emitter.Scale.X;

            Depth = emitter.Depth + .0001f;
            
            Alpha = 0f;

            Angle = (float) (RND.Next * 2 * Math.PI);

            LifeTime = maxLifeTime;

            YVel = -.05f - (float)RND.Next * .3f;

        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            var r = LifeTime / (float)maxLifeTime;

            Scale = new Vector2(r) * initialScale * 2;
            Alpha = (float)Math.Sin(r * Math.PI);
        }
    }

    public class TorchEmitter : ParticleEmitter
    {
        private bool readyToDie;

        public int XRange { get; set; } = 6;
        public int YRange { get; set; } = 8;

        public List<Color> ParticleColors { get; set; } = GameResources.FireColors;

        public TorchEmitter(float x, float y) : base(x, y)
        {
            SpawnRate = 1;
            SpawnTimeout = 4;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (readyToDie && Particles.Count == 0)
                Destroy();
        }

        public override void CreateParticle()
        {
            var part = new TorchParticle(this) { Position = this.Position + new Vector2(-.5f * XRange + RND.Int(XRange), -.5f * YRange + RND.Int(YRange)), Depth = Depth };

            int colorIndex = RND.Int(ParticleColors.Count - 1);
            part.Color = ParticleColors[colorIndex];

        }

        public void Kill()
        {
            Active = false;
            readyToDie = true;
        }
    }
}
