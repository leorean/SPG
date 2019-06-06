using Microsoft.Xna.Framework;
using Platformer.Main;
using SPG.Objects;
using SPG.Util;

namespace Platformer.Objects.Effects.Emitters
{
    public class StarParticle : Particle
    {
        float maxLifeTime = 40f;

        public StarParticle(ParticleEmitter emitter, float initialSpeed) : base(emitter)
        {
            LifeTime = (int)maxLifeTime;
            
            Scale = new Vector2(.3f, .3f);

            Angle = (float)(RND.Next * 360);
            DrawOffset = new Vector2(8, 8);

            float spd = .5f + (float)(RND.Next * initialSpeed);

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

    public class StarEmitter : ParticleEmitter
    {
        private float initialSpeed;

        public StarEmitter(float x, float y, int spawnRate = 6, float initialSpeed = 1.5f) : base(x, y)
        {
            SpawnRate = spawnRate;
            this.initialSpeed = initialSpeed;
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
            var particle = new StarParticle(this, initialSpeed);
            particle.Texture = AssetManager.Particles[0];
            particle.Color = Color;
        }
    }
}
