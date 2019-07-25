using Leore.Main;
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
    public class SnowParticle : Particle
    {
        private bool onGround;

        public SnowParticle(ParticleEmitter emitter) : base(emitter)
        {
            Angle = (float)(RND.Next * 2 * Math.PI);
            Alpha = .5f + (float)RND.Next * .5f;
            LifeTime = 900;
            Scale = new Vector2(1 + (float)RND.Next * 2);

            YVel = .5f;

            //Position = new Vector2(RoomCamera.Current.ViewX + (float)RND.Next * RoomCamera.Current.ViewWidth, RoomCamera.Current.ViewY);
            Position = new Vector2(RoomCamera.Current.CurrentRoom.X + (float)RND.Next * RoomCamera.Current.CurrentRoom.BoundingBox.Width, RoomCamera.Current.CurrentRoom.Y);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!onGround)
            {                
                onGround = GameManager.Current.Map.CollisionTile(X, Y + YVel);
            }
            else
            {
                YVel = 0;
                Alpha = Math.Max(Alpha - .01f, 0);
                if (Alpha == 0)
                    LifeTime = 0;
            }
            
        }
    }

    public class SnowEmitter : ParticleEmitter
    {
        public SnowEmitter(float x, float y) : base(x, y)
        {
            SpawnRate = 5;
            Active = true;
        }

        public override void CreateParticle()
        {
            new SnowParticle(this);
        }
    }
}
