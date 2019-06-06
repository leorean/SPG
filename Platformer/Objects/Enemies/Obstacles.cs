﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Util;

namespace Platformer.Objects.Enemies
{
    public abstract class Obstacle : RoomObject
    {
        public int Damage { get; protected set; } = 1;
        
        public Obstacle(float x, float y, Room room, string name = null) : base(x, y, room, name)
        {
            //Depth = Globals.LAYER_BG - .0005f;
            //Depth = Globals.LAYER_BG2 + .0001f;
            //DebugEnabled = true;
        }
    }

    public class SpikeBottom : Obstacle
    {
        public SpikeBottom(float x, float y, Room room) : base(x, y, room) { BoundingBox = new RectF(2, 8, 12, 8); }        
    }

    public class SpikeTop : Obstacle
    {
        public SpikeTop(float x, float y, Room room) : base(x, y, room) { BoundingBox = new RectF(2, 0, 12, 8); }        
    }

    public class SpikeLeft : Obstacle
    {
        public SpikeLeft(float x, float y, Room room) : base(x, y, room) { BoundingBox = new RectF(8, 2, 8, 12); }        
    }

    public class SpikeRight : Obstacle
    {
        public SpikeRight(float x, float y, Room room) : base(x, y, room) { BoundingBox = new RectF(0, 2, 8, 12); }
    }

    public class BigSpike : SpikeBottom
    {
        public BigSpike(float x, float y, Room room) : base(x, y, room)
        {
            BoundingBox = new RectF(0, 16, 32, 16);            
            Damage = 9999;
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);

        }
    }
}
