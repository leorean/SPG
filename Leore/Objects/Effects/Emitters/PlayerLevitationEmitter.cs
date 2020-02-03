using Microsoft.Xna.Framework;
using SPG.Objects;
using SPG.Util;
using System;
using System.Collections.Generic;

namespace Leore.Objects.Effects.Emitters
{
    public class PlayerLevitationParticle : Particle
    {
        public PlayerLevitationParticle(ParticleEmitter emitter) : base(emitter)
        {
            LifeTime = 30;

            Scale = new Vector2(3, 3);

            Angle = (float)(RND.Next * 360);

            float spd = (float)(1 + RND.Next * .5f);

            float posX = (float)MathUtil.LengthDirX(Angle) * 8;
            float posY = (float)MathUtil.LengthDirY(Angle) * 8;

            Position = new Vector2(emitter.X + posX, emitter.Y + posY);

            YVel = 2;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var s = Math.Max(3 * LifeTime / 30f, 1);

            XVel *= .9f;
            YVel *= .9f;

            Alpha = (float)(Math.Sin(LifeTime / 30f * Math.PI));

            Scale = new Vector2(s);
        }
    }

    public class PlayerLevitationEmitter : ParticleEmitter
    {
        List<Color> particleColors;

        public PlayerLevitationEmitter(float x, float y, GameObject parent) : base(x, y)
        {
            Parent = parent;

            particleColors = new List<Color>
            {
                new Color(255, 255, 255),
                new Color(217, 255, 152),
                new Color(177, 255, 116),
                new Color(131, 237, 100)
            };

            SpawnTimeout = 1;
        }

        public override void CreateParticle()
        {
            var particle = new PlayerLevitationParticle(this);

            var colorIndex = RND.Int(particleColors.Count - 1);
            particle.Color = particleColors[colorIndex];
        }
    }
}
