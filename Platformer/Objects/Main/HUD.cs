using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects.Main
{
    public class HUD
    {
        private int hp;
        private int maxHP;
        private float mp;
        private int maxMP;

        public Texture2D Texture { get; set; }

        private Player player;

        public HUD()
        {

        }

        internal void SetTarget(Player player)
        {
            this.player = player;
        }

        public void Update(GameTime gameTime)
        {
            if (player == null)
                return;

            var stats = player.Stats;
            
            hp = player.HP;
            maxHP = stats.MaxHP;
            mp = player.MP;
            maxMP = stats.MaxMP;
        }

        public void Draw(SpriteBatch sb, GameTime gameTime)
        {
            var x = MainGame.Current.Camera.ViewX + 1;
            var y = MainGame.Current.Camera.ViewY + 1;

            var font = MainGame.Current.HUDFont;

            font.Halign = SPG.Draw.Font.HorizontalAlignment.Left;
            font.Valign = SPG.Draw.Font.VerticalAlignment.Top;

            font.Draw(sb, x, y, "24/50", scale:0.5f);

            /*
            // HP

            sb.Draw(Texture, new Vector2(x, y), new Rectangle(0, 0, 11, 12), Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 1);

            var hpFactor = 3;

            for(var i = 0; i < maxHP * hpFactor; i++)
            {
                var col = 0 + Convert.ToInt32(i < hp * hpFactor);

                if (i == 0 || i == maxHP * hpFactor - 1)
                    col = 2;

                sb.Draw(Texture, new Vector2(x + 12 + i, y), new Rectangle(16 + col, 0, 1, 7), Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 1);
            }

            // magic

            sb.Draw(Texture, new Vector2(x + 12, y + 8), new Rectangle(0, 16, 11, 12), Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 1);

            var mpFactor = .25f;

            for (var i = 0; i < maxMP * mpFactor; i++)
            {
                var col = 0 + Convert.ToInt32(i < mp * mpFactor);

                if (i == 0 || i == maxMP * mpFactor - 1)
                    col = 2;

                sb.Draw(Texture, new Vector2(x + 12 + 12 + i, y + 8), new Rectangle(16 + col, 16, 1, 7), Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 1);
            }*/
        }        
    }
}
