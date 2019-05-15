using Microsoft.Xna.Framework;
using Platformer.Main;
using Platformer.Objects.Main;
using SPG.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects.Effects.Emitters
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
        public FlashEmitter(float x, float y) : base(x, y)
        {
            SpawnRate = 1;
            Texture = AssetManager.Flash;
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
            var flash = new FlashParticle(this);
        }
    }    
}
