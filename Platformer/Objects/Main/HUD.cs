using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Platformer.Main;
using SPG.Draw;
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
        private float coins;

        public Texture2D Texture { get; set; }


        private Font font;
        private Player player;
        
        public HUD()
        {
            font = AssetManager.HUDFontSmall;
            //font = AssetManager.DamageFont;
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
            coins = stats.Coins;            
        }

        public void Draw(SpriteBatch sb, GameTime gameTime)
        {
            var x = RoomCamera.Current.ViewX;
            var y = RoomCamera.Current.ViewY;
            
            //font.Halign = Font.HorizontalAlignment.Left;
            font.Valign = Font.VerticalAlignment.Top;

            //font.Draw(sb, x + 2, y + 2, $"HP: {hp}/{maxHP}", scale: .5f, depth: .991f);
            //font.Draw(sb, x + 2, y + 2 + 9, $"MP: {Math.Floor(mp)}/{maxMP}", scale: .5f, depth: .991f);
            //font.Draw(sb, x + 2, y + 2 + 18, $"Coins: {coins}", scale: .5f, depth: .991f);

            font.Halign = Font.HorizontalAlignment.Center;
            

            font.Draw(sb, x + 24, y + 9.5f, $"{hp}/{maxHP}", scale: .5f, depth: .991f);
            //font.Draw(sb, x + 48, y + 4, $"1{Math.Floor(mp)}/1{maxMP}", scale: .5f, depth: .991f);
            sb.Draw(AssetManager.HUD, new Vector2(x, y), new Rectangle(0, 0, 512, 144), Color.White, 0, Vector2.Zero, new Vector2(.5f), SpriteEffects.None, .990f);

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
