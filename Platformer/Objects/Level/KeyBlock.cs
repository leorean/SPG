using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Platformer.Objects.Main;

namespace Platformer.Objects.Level
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
