using Leore.Main;
using Microsoft.Xna.Framework;
using SPG.Map;
using SPG.Objects;
using SPG.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leore.Objects.Effects.Emitters
{
    public class SnowWeatherParticle : Particle
    {
        private bool onGround;

        private float a;
        private float maxA;
        
        public SnowWeatherParticle(ParticleEmitter emitter) : base(emitter)
        {
            Angle = (float)(RND.Next * 2 * Math.PI);
            maxA = .5f + (float)RND.Next * .5f;
            Alpha = 0;
            LifeTime = 1200;
            Scale = new Vector2(1 + (float)RND.Next * 2);

            YVel = .4f + (float)(RND.Next * .2f);

            if (RoomCamera.Current.CurrentRoom == null)
                LifeTime = 0;
            else
            {
                Position = new Vector2(RoomCamera.Current.CurrentRoom.X - RoomCamera.Current.ViewWidth + (float)RND.Next * (RoomCamera.Current.CurrentRoom.BoundingBox.Width + 2 * RoomCamera.Current.ViewWidth)
                    , RoomCamera.Current.ViewY - (float)RND.Next * RoomCamera.Current.ViewHeight);
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            if (!onGround)
            {
                if (Y > RoomCamera.Current.ViewY)
                    onGround = GameManager.Current.Map.CollisionTile(X, Y + YVel) || GameManager.Current.Map.CollisionTile(X, Y - YVel, GameMap.WATER_INDEX);
            }
            else
            {
                YVel = 0;
                maxA = Math.Max(maxA - .01f, 0);                
            }

            if (LifeTime < 100)
                maxA = Math.Max(maxA - .01f, 0);

            if (maxA == 0)
                LifeTime = 0;

            if (Y > RoomCamera.Current.ViewY + 2 && (GameManager.Current.Map.CollisionTile(X, Y - 2f) || GameManager.Current.Map.CollisionTile(X, Y - 2f, GameMap.WATER_INDEX)))
                LifeTime = 0;

            if (Y > RoomCamera.Current.ViewY + 2)
                a = Math.Min(a + .02f, maxA);

            Alpha = a;
        }
    }

    public class SnowWeatherEmitter : ParticleEmitter
    {
        public SnowWeatherEmitter(float x, float y) : base(x, y)
        {
            SpawnRate = 20;
            Active = true;
        }

        public override void CreateParticle()
        {
            if (Particles.Count > 1500)
                return;

            new SnowWeatherParticle(this);
        }

        public override void Destroy(bool callGC = false)
        {
            base.Destroy(callGC);
        }
    }
}
