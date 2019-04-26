using Microsoft.Xna.Framework;
using Platformer.Objects;
using SPG.Objects;
using System;

namespace Platformer
{
    public class Solid : RoomDependentdObject
    {
        public Solid(float x, float y, Room room) : base(x, y, room)
        {
            Name = "solid";

            BoundingBox = new SPG.Util.RectF(0, 0, Globals.TILE, Globals.TILE);
            Visible = false;            
        }
        /*public Solid(int x, int y, Room room)
        {
            Name = "solid";
            Position = new Vector2(x, y);
            BoundingBox = new SPG.Util.RectF(0, 0, Globals.TILE, Globals.TILE);
            Visible = false;

            Room = room;
        }*/
    }
}