using Microsoft.Xna.Framework;
using SPG.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SPG.Map;
using SPG.Util;

namespace Leore.Objects.Effects.Emitters
{
    public class FlowParticle : Particle
    {
        public FlowParticle(FlowEmitter emitter) : base(emitter as ParticleEmitter)
        {
            Scale = new Vector2(2);
            LifeTime = 30;
            
            switch (emitter.direction)
            {
                case Direction.NONE:
                    break;
                case Direction.LEFT:
                    Position = emitter.Position + new Vector2(Globals.T, (float)RND.Next * Globals.T);
                    XVel = -1;                    
                    break;
                case Direction.RIGHT:
                    Position = emitter.Position + new Vector2(0, (float)RND.Next * Globals.T);
                    XVel = 1;
                    break;
                case Direction.UP:
                    Position = emitter.Position + new Vector2((float)RND.Next * Globals.T, Globals.T);
                    YVel = -1;
                    break;
                case Direction.DOWN:
                    Position = emitter.Position + new Vector2((float)RND.Next * Globals.T, 0);
                    YVel = 1;
                    break;
                default:
                    break;
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Scale = new Vector2(2 * LifeTime / 30f);

            Alpha = .5f * (float)Math.Sin(LifeTime / 30f * Math.PI);
        }
    }

    public class FlowEmitter : ParticleEmitter
    {
        public Direction direction;

        public FlowEmitter(float x, float y, Direction direction) : base(x, y)
        {
            this.direction = direction;
            SpawnRate = 1;
        }

        public override void CreateParticle()
        {
            new FlowParticle(this);        
        }
    }
}
