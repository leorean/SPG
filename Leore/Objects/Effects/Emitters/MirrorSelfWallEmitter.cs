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
    public class MirrorSelfParticle : Particle
    {
        private bool wasGlowing;

        public MirrorSelfParticle(ParticleEmitter emitter) : base(emitter)
        {
            Scale = new Vector2(1 + 1.5f * (float)RND.Next);
            Angle = (float)RND.Next * 360;
            LifeTime = 180;

            Alpha = 0;

            XVel = -.25f + (float)RND.Next * .5f;            
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!wasGlowing)
            {
                Alpha = Math.Min(Alpha + .1f, 1);
                if (Alpha == 1)
                    wasGlowing = true;
            }
            else
            {
                Alpha = Math.Max(Alpha - .01f, 0);
                if (Alpha == 0)
                    LifeTime = 0;
            }
        }
    }

    public class MirrorSelfWallEmitter : ParticleEmitter
    {
        private Room room;

        public MirrorSelfWallEmitter(float x, float y, Room room) : base(x, y)
        {
            this.room = room;
            SpawnRate = 1;
        }

        public override void CreateParticle()
        {
            var part = new MirrorSelfParticle(this);
            part.Position = new Vector2(room.X + room.BoundingBox.Width * .5f, room.Y + (float)RND.Next * room.BoundingBox.Height);
        }
    }
}
