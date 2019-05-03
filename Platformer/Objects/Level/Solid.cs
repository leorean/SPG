using Microsoft.Xna.Framework;
using Platformer.Objects;
using SPG.Objects;
using System;

namespace Platformer.Objects.Level
{

    public abstract class Collider : RoomObject
    {
        public Collider(float x, float y, Room room) : base(x, y, room) { }
    }

    public class Platform : Collider
    {
        public Platform(float x, float y, Room room) : base(x, y, room)
        {            
            BoundingBox = new SPG.Util.RectF(0, 0, Globals.TILE, 1);
            Visible = false;
        }
    }

    public class Solid : Collider
    {
        public Solid(float x, float y, Room room) : base(x, y, room)
        {            
            BoundingBox = new SPG.Util.RectF(0, 0, Globals.TILE, Globals.TILE);
            Visible = false;
        }
    }
}