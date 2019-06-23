using Microsoft.Xna.Framework;
using Leore.Main;
using SPG.Map;
using SPG.Util;

namespace Leore.Objects.Level
{

    public abstract class Collider : RoomObject
    {
        public Collider(float x, float y, Room room) : base(x, y, room)
        {
            //Room.Colliders.Add(this);
        }
    }

    public class Platform : Collider
    {
        public Platform(float x, float y, Room room) : base(x, y, room)
        {            
            BoundingBox = new SPG.Util.RectF(0, 0, Globals.T, 1);
            Visible = false;
        }
    }

    public class Solid : Collider
    {
        public Solid(float x, float y, Room room) : base(x, y, room)
        {
            BoundingBox = new SPG.Util.RectF(0, 0, Globals.T, Globals.T);
            Visible = false;
        }
        public override void BeginUpdate(GameTime gameTime)
        {
            base.BeginUpdate(gameTime);

            int tx = MathUtil.Div(X, Globals.T);
            int ty = MathUtil.Div(Y, Globals.T);

            var val = GameManager.Current.Map.LayerData[GameMap.FG_INDEX].Get(tx, ty);
            if (val?.TileOptions != null)
                val.TileOptions.Solid = true;
        }

        public override void Destroy(bool callGC = false)
        {
            int tx = MathUtil.Div(X, Globals.T);
            int ty = MathUtil.Div(Y, Globals.T);

            var val = GameManager.Current.Map.LayerData[GameMap.FG_INDEX].Get(tx, ty);
            if (val?.TileOptions != null)
                val.TileOptions.Solid = false;

            base.Destroy(callGC);
        }        
    }
}