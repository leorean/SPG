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
        public int Background { get; set; } = -1;
        
        public Room(int x, int y, int width, int height) : base(x, y, "room")
        {
            BoundingBox = new RectF(0, 0, width, height);            
            Visible = false;            
        }
    }
}
