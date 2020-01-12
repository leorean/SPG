using Leore.Main;
using SPG.Objects;

namespace Leore.Objects
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
        
        public override long ID
        {
            get => base.ID + RoomID();
            //set => id = value + RoomID();
        }

        long RoomID()
        {
            var hc = GameManager.Current.Map.Name.GetHashCode();
            return hc;
        }
    }    
}
