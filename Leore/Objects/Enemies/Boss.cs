using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leore.Main;
using Leore.Resources;
using Leore.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Leore.Objects.Enemies
{
    public class Boss : Enemy
    {
        public Boss(float x, float y, Room room) : base(x, y, room)
        {
        }
        
        public override void OnDeath()
        {
            //base.OnDeath();
            GameManager.Current.Player.Stats.Bosses.Add(ID);
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);
            
            var pos = new Vector2(Room.X + Room.BoundingBox.Width * .5f, Room.Y + Room.BoundingBox.Height - .5f * Globals.TILE);
            sb.DrawBar(pos, 5 * Globals.TILE, (float)HP / (float)MaxHP, GameResources.HpColors[1], Color.Black, Globals.LAYER_UI, 6, true, Color.Black);
        }
    }
}
