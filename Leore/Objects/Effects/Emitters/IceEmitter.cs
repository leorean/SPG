using Leore.Main;
using Leore.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Objects;
using SPG.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leore.Objects.Effects.Emitters
{
    public class IceParticle : Particle
    {
        private float frame;
        private float minFrame;
        private float maxFrame;
        private float fSpd;

        public IceParticle(ParticleEmitter emitter) : base(emitter)
        {
            DrawOffset = new Vector2(8);
            LifeTime = 60;

            minFrame = 17;
            maxFrame = 20;
            frame = minFrame;
            fSpd = .1f;

            Texture = AssetManager.Particles[17 + RND.Int(3)];

            Position = emitter.Position + new Vector2(-4 + RND.Int(8), -4 + RND.Int(8));            
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            //frame = ((frame + fSpd) % (maxFrame - minFrame));
            //frame = Math.Min(frame + fSpd, maxFrame - 1);

            //Texture = AssetManager.Particles[(int)frame];

            Alpha = LifeTime / 60f;

            Scale = new Vector2(.5f + (LifeTime / 60f) * .5f);

            YVel = Math.Min(YVel + .01f, 1);
            
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);
        }
    }

    public class IceEmitter : ParticleEmitter
    {
        private List<Color> particleColors;

        public IceEmitter(float x, float y) : base(x, y)
        {
            particleColors = GameResources.MpColors;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Particles.Count == 0)
                Destroy();
        }

        public override void CreateParticle()
        {
            new IceParticle(this);
        }
    }
}
