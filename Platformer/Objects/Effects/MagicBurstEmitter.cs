using Microsoft.Xna.Framework;
using SPG.Objects;
using SPG.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects.Effects
{
    public class MagicBurstEmitter : ParticleEmitter
    {
        public List<Color> ParticleColors;

        public MagicBurstEmitter(float x, float y) : base(x, y)
        {
            ParticleColors = new List<Color>();
            ParticleColors.Add(new Color(255, 255, 255));
            ParticleColors.Add(new Color(206, 255, 255));
            ParticleColors.Add(new Color(168, 248, 248));
            ParticleColors.Add(new Color(104, 216, 248));

            SpawnRate = 50;

            ParticleInit = (particle) =>
            {
                particle.LifeTime = 30;

                SpawnRate = 0;
                
                particle.Scale = new Vector2(3, 3);

                particle.Angle = (float)(RND.Next * 360);

                float spd = (float)(1 + RND.Next * .5f);

                particle.XVel = (float)MathUtil.LengthDirX(particle.Angle) * spd;
                particle.YVel = (float)MathUtil.LengthDirY(particle.Angle) * spd;

                var colorIndex = RND.Int(ParticleColors.Count - 1);
                particle.Color = ParticleColors[colorIndex];
            };

            ParticleUpdate = (particle) =>
            {
                var s = Math.Max(3 * particle.LifeTime / 30f, 1);

                particle.XVel *= .9f;
                particle.YVel *= .9f;

                particle.Alpha = particle.LifeTime / 15f;

                particle.Scale = new Vector2(s);
            };
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (particles.Count == 0)
                Destroy();
        }
    }
}
