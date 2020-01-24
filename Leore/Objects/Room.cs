using Leore.Main;
using Microsoft.Xna.Framework;
using SPG.Objects;
using SPG.Util;

namespace Leore.Objects
{
    public class Room : SpatialGameObject
    {
        public int Background { get; set; } = -1;
        public int Weather { get; set; } = -1;
        public bool IsDark { get; set; } = false;

        public bool SwitchState { get; set; }
        
        public Room(int x, int y, int width, int height, int mapIndex) : base(x, y, mapIndex, "room")
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
