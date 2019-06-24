using Microsoft.Xna.Framework;
using SPG.Objects;
using SPG.Util;

namespace Leore.Objects
{
    public class Room : GameObject
    {
        public int Background { get; set; } = -1;
        public int Weather { get; set; } = -1;

        public bool SwitchState { get; set; }

        public Room(int x, int y, int width, int height) : base(x, y, "room")
        {
            BoundingBox = new RectF(0, 0, width, height);            
            Visible = false;            
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
    }
}
