using Leore.Main;
using Microsoft.Xna.Framework;

namespace Leore.Objects.Level
{
    public class JumpControlDisabler : RoomObject
    {
        private string disappearCondition;

        public JumpControlDisabler(float x, float y, Room room, string disappearCondition = null) : base(x, y, room)
        {
            this.disappearCondition = disappearCondition;
            BoundingBox = new SPG.Util.RectF(0, 0, Globals.TILE, Globals.TILE);
            Visible = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (disappearCondition != null && GameManager.Current.HasStoryFlag(disappearCondition))
                Destroy();
        }
    }
}
