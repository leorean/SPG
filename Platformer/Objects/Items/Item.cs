using Microsoft.Xna.Framework;
using SPG.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects.Items
{
    /*
    
        item concept:

        item floats around somewhere, if player takes it, he holds it up (state)
     
         
         take -> add to player.Stats.items!!!
         
        spawn: check if exists ID in player.stats.items, and delete if yes
         
    */

    public abstract class Item : RoomObject
    {
        // properties

        // public bool Taken { get; private set; }

        // constructor

        public Item(float x, float y, Room room, string name = null) : base(x, y, room, name)
        {
            DrawOffset = new Vector2(8, 8);
            BoundingBox = new SPG.Util.RectF(-4, -4, 8, 8);
            Depth = Globals.LAYER_ITEM;
        }

        // methods

        public abstract void Take();
    }
}
