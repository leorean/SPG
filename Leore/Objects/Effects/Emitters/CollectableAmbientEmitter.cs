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
    public class CollectableAmbientParticle : Particle
    {
        float angle;
        readonly float maxLifeTime = 60;
        float alpha;

        public CollectableAmbientParticle(ParticleEmitter emitter, float angle) : base(emitter)
        {
            LifeTime = (int)maxLifeTime;
            this.angle = angle;
            Scale = new Vector2(3);

            Angle = (float)MathUtil.DegToRad(angle + 45);
        }

        public override void Update(GameTime gameTime)
        {

            base.Update(gameTime);

            float life = LifeTime / maxLifeTime;

            if (life < .5f)
                alpha = life;
            else
                alpha = 1 - life;

            //alpha = (float)Math.Sin(.5f * life * Math.PI) - .3f;

            Scale = new Vector2(Math.Max(Scale.X - .04f, 1));

            var vel = (life - .3f);

            XVel = (float)MathUtil.LengthDirX(angle) * vel;
            YVel = (float)MathUtil.LengthDirY(angle) * vel;

            Alpha = alpha;
        }
    }

    public class CollectableAmbientEmitter : ParticleEmitter
    {
        float angle;
        public CollectableAmbientEmitter(float x, float y) : base(x, y)
        {
            SpawnRate = 1;
            SpawnTimeout = 4;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            angle = (angle + 5) % (360);
        }

        public override void CreateParticle()
        {
            new CollectableAmbientParticle(this, angle);
            new CollectableAmbientParticle(this, angle + 120);
            new CollectableAmbientParticle(this, angle + 240);
        }
    }
}
