using Leore.Main;
using SPG.Objects;
using SPG.Util;

namespace Leore.Objects
{
    public static class IDHelper
    {
        public static long GetUniqueIDForCurrentMap(long myID)
        {
            return long.Parse($"{myID}" + $"{RoomID()}");
        }

        static long RoomID()
        {
            return GameManager.Current.Map.Name.GetUniqueHashCode();            
        }
    }

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
            get => IDHelper.GetUniqueIDForCurrentMap(base.ID);
        }        
    }    
}
