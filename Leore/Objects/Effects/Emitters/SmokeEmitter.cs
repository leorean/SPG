using Microsoft.Xna.Framework;
using SPG.Objects;
using SPG.Util;

namespace Leore.Objects.Effects.Emitters
{
    public class SmokeParticle : Particle
    {
        float alpha = .5f;
        float maxLifeTime = 60;

        public SmokeParticle(ParticleEmitter emitter) : base(emitter)
        {
            LifeTime = (int)maxLifeTime;
            Alpha = alpha;

            Scale = new Vector2(1.5f + (float)RND.Next * 1f);

            Position = emitter.Position + new Vector2(-5 + RND.Int(10), 0);
            Depth = emitter.Depth;

            XVel = -.2f + (float)RND.Next * .4f;
            YVel = -.4f;

            var val = 200 + RND.Int(55);
            Color = new Color(val, val, val);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var s = Scale.X + .03f;
            var a = Alpha - .001f;

            Scale = new Vector2(s);
            Alpha = (LifeTime / maxLifeTime) * alpha;
            
        }
    }

    public class SmokeEmitter : ParticleEmitter
    {
        public SmokeEmitter(float x, float y) : base(x, y)
        {
            Depth = Globals.LAYER_FG + .0001f;
            SpawnRate = 2;
        }

        public override void CreateParticle()
        {
            new SmokeParticle(this);
        }
    }

    public class Smoke : RoomObject
    {
        private SmokeEmitter emitter;
        public Smoke(float x, float y, Room room) : base(x, y, room)
        {
            Depth = Globals.LAYER_BG2 - .0001f;
            emitter = new SmokeEmitter(x, y);
            emitter.Parent = this;
            emitter.Depth = Depth;
        }
    }
}
