using Platformer.Main;
using SPG.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects
{
    public enum Direction
    {
        NONE = 0,
        LEFT = -1,
        RIGHT = 1,
        UP = -2,
        DOWN = 2
    }    

    public static class ObjectExtensions
    {
        public static bool IsOutsideCurrentRoom(this GameObject o)
        {
            var cam = RoomCamera.Current;
            
            if (cam == null)
                return true;

            //return o.X < cam.ViewX || o.Y < cam.ViewY || o.X > cam.ViewX + cam.ViewWidth || o.Y > cam.ViewY + cam.ViewHeight;

            if (cam.CurrentRoom == null)
                return false;

            return o.X < cam.CurrentRoom.X || o.Y < cam.CurrentRoom.Y || o.X > cam.CurrentRoom.X + cam.CurrentRoom.BoundingBox.Width || o.Y > cam.CurrentRoom.Y + cam.CurrentRoom.BoundingBox.Height;
        }
    }
}
