using System.Linq;
using Microsoft.Xna.Framework;
using Leore.Main;

namespace Leore.Objects.Level
{
    public class KeyBlock : Solid
    {
        public KeyBlock(float x, float y, Room room) : base(x, y, room)
        {
            Texture = GameManager.Current.Map.TileSet[582];            
        }

        public override void BeginUpdate(GameTime gameTime)
        {
            base.BeginUpdate(gameTime);

            if (GameManager.Current.Player.Stats.KeysAndKeyblocks.Contains(ID))
            {
                Destroy();
                return;
            }

            Visible = true;
        }
    }
}
