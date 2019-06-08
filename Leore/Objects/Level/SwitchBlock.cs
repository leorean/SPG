using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Leore.Main;
using SPG.Objects;

namespace Leore.Objects.Level
{
    public class SwitchBlock : RoomObject
    {
        public int Type { get; private set; }

        public bool Active { get; private set; }

        private Texture2D texActive = GameManager.Current.Map.TileSet[719];
        private Texture2D texInactive = GameManager.Current.Map.TileSet[720];

        private Solid block;

        public SwitchBlock(float x, float y, Room room, int type) : base(x, y, room)
        {
            Type = type;
            Depth = Globals.LAYER_FG;
            Visible = true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var switchState = Room.SwitchState;

            if (Type == 0) {
                Active = switchState;
            }
            else
            {
                Active = !switchState;
            }

            //if (ObjectManager.CollisionRectangle(GameManager.Current.Player, X, Y, X + 16, Y + 16))
            if (Active && this.CollisionBounds(GameManager.Current.Player, X, Y))
                Active = false;
            
            if (Active && block == null)
            {                
                block = new Solid(X, Y, Room);
            }
            if (!Active && block != null)
            {
                block.Destroy();
                block = null;
            }

            if (!Active) Texture = texInactive;
            if (Active) Texture = texActive;
        }
    }
}
