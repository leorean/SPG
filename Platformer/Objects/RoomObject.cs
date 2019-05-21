using Platformer.Main;
using Platformer.Objects.Level;
using SPG.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects
{
    public abstract class RoomObject : GameObject
    {
        public Room Room { get; set; }

        public RoomObject(float x, float y, Room room, string name = null) : base(x, y, name)
        {
            Room = room;
            Position = new Microsoft.Xna.Framework.Vector2(x, y);
            Depth = Globals.LAYER_BG + .001f;            
        }
    }    
}
