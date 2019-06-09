using Microsoft.Xna.Framework;
using Leore.Main;
using SPG.Objects;
using System;

namespace Leore.Objects.Effects.Emitters
{
    public class FlashParticle : Particle
    {
        public FlashParticle(ParticleEmitter emitter) : base(emitter)
        {
            Depth = Globals.LAYER_UI + .001f;
            Alpha = 1.5f;

            LifeTime = 90;

            Texture = emitter.Texture;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Alpha = Math.Max(Alpha - .05f, 0);

            if (Alpha == 0)
                LifeTime = 0;
            
            Position = new Vector2(RoomCamera.Current.ViewX, RoomCamera.Current.ViewY);
        }
    }

    public class FlashEmitter : ParticleEmitter
    {
        private int delay;

        public FlashEmitter(float x, float y, int delay = 0) : base(x, y)
        {
            SpawnRate = 1;
            Texture = AssetManager.Flash;
            this.delay = delay;
        }

        public override void Update(GameTime gameTime)
        {
            if(delay > 0)
            {
                delay = Math.Max(delay - 1, 0);
                return;
            }

            base.Update(gameTime);

            SpawnRate = 0;

            if (Particles.Count == 0)
                Destroy();
        }

        public override void CreateParticle()
        {
            var flash = new FlashParticle(this);
        }
    }
}
