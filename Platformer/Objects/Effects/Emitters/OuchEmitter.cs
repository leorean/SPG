using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Platformer.Objects.Level;
using SPG.Objects;
using SPG.Util;
using SPG.Draw;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platformer.Objects.Main;

namespace Platformer.Objects.Effects.Emitters
{
    public class OuchParticle : Particle
    {
        float maxLifeTime = 40f;

        public OuchParticle(ParticleEmitter emitter) : base(emitter)
        {
            LifeTime = (int)maxLifeTime;

            Scale = new Vector2(.3f, .3f);

            Angle = (float)(RND.Next * 360);
            DrawOffset = new Vector2(8, 8);

            float spd = .5f + (float)(RND.Next * 1.5f);

            XVel = (float)MathUtil.LengthDirX(Angle) * spd;
            YVel = (float)MathUtil.LengthDirY(Angle) * spd;
            
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            XVel *= .9f;
            YVel *= .9f;

            Alpha = LifeTime / maxLifeTime;            
        }        
    }

    public class OuchEmitter : ParticleEmitter
    {
        public OuchEmitter(float x, float y) : base(x, y)
        {
            SpawnRate = 6;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            SpawnRate = 0;

            if (particles.Count == 0)
                Destroy();
        }

        public override void CreateParticle()
        {
            var particle = new OuchParticle(this);
            particle.Texture = AssetManager.OuchSprite;
        }
    }
}
