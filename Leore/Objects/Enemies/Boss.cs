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
using SPG;

namespace Leore.Objects.Enemies
{
    public class Boss : Enemy
    {
        private string setCondition;

        public Boss(float x, float y, Room room, string setCondition) : base(x, y, room)
        {
            this.setCondition = setCondition;
            MainGame.Current.HUD.SetBoss(this);
        }

        public override void Hit(int hitPoints, float degAngle)
        {
            base.Hit(hitPoints, degAngle);
            MainGame.Current.HUD.SetBoss(this);
        }

        public override void OnDeath()
        {
            //base.OnDeath();

            GameManager.Current.AddStoryFlag(setCondition);

            GameManager.Current.Player.Stats.Bosses.Add(ID);
            MainGame.Current.HUD.SetBoss(null);
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);
            
            //var pos = new Vector2(Room.X + Room.BoundingBox.Width * .5f, Room.Y + Room.BoundingBox.Height - 12);
            //sb.DrawBar(pos, 7 * Globals.TILE, (float)HP / (float)MaxHP, GameResources.HpColors[1], Color.Black, Globals.LAYER_UI, 6, false, Color.Black);
            //sb.DrawBar(pos + new Vector2(0, 2), 7 * Globals.TILE, (float)HP / (float)MaxHP, Colors.FromHex("a00e12"), Color.Black, Globals.LAYER_UI - .0001f, 6, false, Color.Black);
        }
    }
}
