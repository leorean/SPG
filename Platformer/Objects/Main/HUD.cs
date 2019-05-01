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
            var x = MainGame.Current.Camera.ViewX;
            var y = MainGame.Current.Camera.ViewY;

            var font = MainGame.Current.HUDFont;

            font.Halign = SPG.Draw.Font.HorizontalAlignment.Left;
            font.Valign = SPG.Draw.Font.VerticalAlignment.Top;

            font.Draw(sb, x + 2, y + 2, $"HP: {hp}/{maxHP}", scale: .5f, depth: .991f);
            font.Draw(sb, x + 2, y + 2 + 9, $"MP: {mp}/{maxMP}", scale: .5f, depth: .991f);

            //font.Draw(sb, x, y, "24/50", scale:0.5f);

            // HP


            /*
            var width = 5 * Globals.TILE;

            for(var i = 0; i < width; i++)
            {
                float t1 = (float)i / (float)width;
                float t2 = (float)hp / (float)maxHP;

                var row = 1 - Convert.ToInt32(t1 < t2);
                var col = 1;

                if (i == 0)
                    col = 0;
                if(i == width - 1)
                    col = 2;

                sb.Draw(Texture, new Vector2(x + i * scale, y), new Rectangle(col, 16 * row, 1, 10), Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, .99f);                
            }

            /*
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
