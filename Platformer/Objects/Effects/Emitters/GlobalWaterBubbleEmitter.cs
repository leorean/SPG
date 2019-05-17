using Microsoft.Xna.Framework;
using Platformer.Main;
using Platformer.Objects.Level;
using Platformer.Objects.Main;
using SPG.Objects;
using SPG.Util;
using SPG.Map;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects.Effects.Emitters
{
    public class WaterBubbleParticle : Particle
    {
        private float sinus;

        public WaterBubbleParticle(GlobalWaterBubbleEmitter emitter) : base(emitter)
        {
            var room = RoomCamera.Current.CurrentRoom;

            if (room == null)
                LifeTime = 0;
            else
            {
                var posX = room.X + RND.Int((int)room.BoundingBox.Width);
                var posY = room.Y + RND.Int((int)room.BoundingBox.Height);

                int tx = MathUtil.Div(posX, Globals.TILE);
                int ty = MathUtil.Div(posY, Globals.TILE);

                var inWater = (GameManager.Current.Map.LayerData[2].Get(tx, ty) != null);

                if (ObjectManager.CollisionPoints<Solid>(posX, posY).Count > 0)
                {
                    inWater = false;
                }

                Alpha = 0;

                if (!inWater)
                    LifeTime = 0;
                else
                {
                    Scale = new Vector2(.5f, .5f);
                    LifeTime = 600;
                    Position = new Vector2(posX, posY);

                    sinus = (float)(RND.Next * 2 * Math.PI);
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            sinus = (float)((sinus + .1f) % (2 * Math.PI));

            YVel = Math.Max(YVel - .01f + (float)(RND.Next * .01f), -.25f);
            XVel = (float)Math.Sin(sinus) * .05f;

            XVel = XVel.Clamp(-.25f, .25f);

            var s = Math.Min(Scale.X + .015f, 2);
            var a = Math.Min(Alpha + .015f, .8f);

            Scale = new Vector2(s);
            Alpha = a;
            
            // destroy:

            if (GameManager.Current.Map.CollisionTile(Position.X, Position.Y))
                LifeTime = 0;
            var inWater = GameManager.Current.Map.CollisionTile(Position.X, Position.Y, GameMap.WATER_INDEX);
                
            if (!inWater)
                LifeTime = 0;
        }
    }

    public class GlobalWaterBubbleEmitter : ParticleEmitter
    {
        private Room room;
        private int waterCount;

        public GlobalWaterBubbleEmitter(float x, float y, GameObject parent) : base(x, y)
        {
            Parent = parent;            
        }

        public override void CreateParticle()
        {
            var part = new WaterBubbleParticle(this);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var roomSize = 0;
            if (RoomCamera.Current.CurrentRoom != null)
            {
                roomSize = MathUtil.Div(RoomCamera.Current.CurrentRoom.BoundingBox.Width, RoomCamera.Current.ViewWidth) 
                    * MathUtil.Div(RoomCamera.Current.CurrentRoom.BoundingBox.Height, RoomCamera.Current.ViewHeight);                
            }

            // measure how many water tiles there are
            if (room != RoomCamera.Current.CurrentRoom && RoomCamera.Current.CurrentRoom != null)
            {
                room = RoomCamera.Current.CurrentRoom;
                
                var posX = room.X;
                var posY = room.Y;

                waterCount = 0;
                for (int i = (int)room.X; i < room.X + room.BoundingBox.Width; i += Globals.TILE)
                { 
                    for (int j = (int)room.Y; j < room.Y + room.BoundingBox.Height; j += Globals.TILE)
                    {
                        int tx = MathUtil.Div(i, Globals.TILE);
                        int ty = MathUtil.Div(j, Globals.TILE);

                        if (GameManager.Current.Map.LayerData[GameMap.WATER_INDEX].Get(tx, ty) != null)
                        {
                            waterCount++;
                        }
                    }
                }
            }

            SpawnRate = (int)Math.Ceiling(roomSize * .5f);
            SpawnTimeout = 1;

            if (Particles.Count >= waterCount)
                SpawnRate = 0;
        }
    }
}
