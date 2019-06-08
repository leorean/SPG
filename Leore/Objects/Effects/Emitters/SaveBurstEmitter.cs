using Microsoft.Xna.Framework;
using SPG.Objects;
using SPG.Util;
using System;
using System.Collections.Generic;

namespace Leore.Objects.Effects.Emitters
{
    public class SaveBurstParticle : Particle
    {
        public SaveBurstParticle(ParticleEmitter emitter) : base(emitter)
        {
            LifeTime = 30;

            Scale = new Vector2(3, 3);

            Angle = (float)(RND.Next * 360);

            float spd = (float)(1 + RND.Next * .5f);

            XVel = (float)MathUtil.LengthDirX(Angle) * spd;
            YVel = (float)MathUtil.LengthDirY(Angle) * spd;            
        }
        
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var s = Math.Max(3 * LifeTime / 30f, 1);

            XVel *= .9f;
            YVel *= .9f;

            Alpha = LifeTime / 15f;

            Scale = new Vector2(s);
        }
    }

    public class SaveBurstEmitter : ParticleEmitter
    {
        public List<Color> ParticleColors { get; set; }

        public SaveBurstEmitter(float x, float y) : base(x, y)
        {
            SpawnRate = 25;

            ParticleColors = new List<Color>
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

            SpawnRate = 0;

            if (Particles.Count == 0)
                Destroy();
        }

        public override void CreateParticle()
        {
            var particle = new SaveBurstParticle(this);

            var colorIndex = RND.Int(ParticleColors.Count - 1);
            particle.Color = ParticleColors[colorIndex];
        }
    }
}
