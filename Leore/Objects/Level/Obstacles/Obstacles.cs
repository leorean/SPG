using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Util;

namespace Leore.Objects.Level.Obstacles
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

    public class ObstacleBlock : Obstacle
    {
        public ObstacleBlock(float x, float y, Room room) : base(x, y, room)
        {
            BoundingBox = new RectF(-1f, -1f, 18, 18);            
        }
    }

    public class SpikeCorner : Obstacle
    {
        public SpikeCorner(float x, float y, Room room) : base(x, y, room) { BoundingBox = new RectF(4, 4, 8, 8); }
    }

    public class SpikeBottom : Obstacle
    {
        public SpikeBottom(float x, float y, Room room) : base(x, y, room) { BoundingBox = new RectF(2, 10, 12, 6); }
    }

    public class SpikeTop : Obstacle
    {
        public SpikeTop(float x, float y, Room room) : base(x, y, room) { BoundingBox = new RectF(2, 0, 12, 6); }        
    }

    public class SpikeLeft : Obstacle
    {
        public SpikeLeft(float x, float y, Room room) : base(x, y, room) { BoundingBox = new RectF(10, 2, 6, 12); }        
    }

    public class SpikeRight : Obstacle
    {
        public SpikeRight(float x, float y, Room room) : base(x, y, room) { BoundingBox = new RectF(0, 2, 6, 12); }
    }

    public class BigSpike : SpikeBottom
    {
        public BigSpike(float x, float y, Room room) : base(x, y, room)
        {
            BoundingBox = new RectF(0, 16, 32, 16);            
            Damage = 999;
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);

        }
    }
}
