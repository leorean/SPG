using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leore.Objects.Level
{
    public class FallOutOfScreenObject : RoomObject
    {
        public FallOutOfScreenObject(float x, float y, Room room) : base(x, y, room)
        {
            Depth = Globals.LAYER_FG;
            BoundingBox = new SPG.Util.RectF(0, 14, 16, 2);
        }
    }
}
