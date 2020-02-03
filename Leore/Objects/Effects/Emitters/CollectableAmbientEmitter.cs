using Leore.Objects.Items;
using Microsoft.Xna.Framework;
using SPG;
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
        readonly float maxLifeTime = 40;
        float alpha;

        public CollectableAmbientParticle(ParticleEmitter emitter) : base(emitter)
        {
            LifeTime = (int)maxLifeTime;
            Scale = new Vector2(3);

            Angle = (float)MathUtil.DegToRad(RND.Next * 360);

            var rx = 12;
            var ry = 5;
            Position = new Vector2(emitter.X - .5f * rx + (float)RND.Next * rx,
                emitter.Y - .5f * ry + (float)RND.Next * ry);

            YVel = (float) (.5f + RND.Next * .25f);

            Color = CollectableAmbientEmitter.GetRandomColor();
        }

        public override void Update(GameTime gameTime)
        {

            base.Update(gameTime);

            float life = LifeTime / maxLifeTime;

            if (life > .75f)
                alpha = Math.Min(alpha + .1f, .8f);
            else
                alpha = Math.Max(alpha - .02f, 0);
            
            Scale = new Vector2(Math.Max(Scale.X - .04f, 1));
            Alpha = alpha;

            YVel = Math.Max(YVel - .1f, -1);
        }
    }

    public class CollectableAmbientEmitter : ParticleEmitter
    {
        public CollectableAmbientEmitter(float x, float y) : base(x, y)
        {
            SpawnRate = 1;
            SpawnTimeout = 6;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void CreateParticle()
        {
            new CollectableAmbientParticle(this);
        }

        public static Color GetRandomColor()
        {
            var min = 160;
            var max = 255 - min;
            var r = min + (int)(RND.Next * max);
            var g = min + (int)(RND.Next * max);
            var b = min + (int)(RND.Next * max);

            return new Color(r, g, b);
        }
    }
}
