using Microsoft.Xna.Framework;
using SPG.Objects;
using SPG.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects
{
    public class Room : GameObject
    {
        public int Background { get; set; }

        private Room() { }

        public Room(int x, int y, int width, int height)
        {
            Position = new Vector2(x, y);
            BoundingBox = new RectF(0, 0, width, height);
            Name = "room";
            Visible = false;
        }
    }
}
