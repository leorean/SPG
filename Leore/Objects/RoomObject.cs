using Leore.Main;
using SPG.Objects;
using SPG.Util;

namespace Leore.Objects
{
    public abstract class RoomObject : MapCoherentGameObject
    {
        public Room Room { get; set; }

        public RoomObject(float x, float y, Room room, string name = null) : base(x, y, GameManager.Current.Map.MapIndex, name)
        {
            Room = room;
            Position = new Microsoft.Xna.Framework.Vector2(x, y);
            Depth = Globals.LAYER_BG + .001f;            
        }         
    }    
}
