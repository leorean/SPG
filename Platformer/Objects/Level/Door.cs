using Platformer.Objects.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects.Level
{
    public class Door : RoomObject
    {
        public int Tx { get; private set; }
        public int Ty { get; private set; }

        public Door(float x, float y, Room room, int tx, int ty, string name = null) : base(x, y, room, name)
        {
            Depth = Globals.LAYER_BG + .001f;
            
            BoundingBox = new SPG.Util.RectF(6, 0, 4, 16);
            Texture = AssetManager.DoorSprite;

            Tx = tx;
            Ty = ty;
        }
    }
}
