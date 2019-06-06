namespace Platformer.Objects.Level
{
    public class JumpControlDisabler : RoomObject
    {
        public JumpControlDisabler(float x, float y, Room room, string name = null) : base(x, y, room, name)
        {
            BoundingBox = new SPG.Util.RectF(0, 0, Globals.TILE, Globals.TILE);
            Visible = false;
        }
    }
}
