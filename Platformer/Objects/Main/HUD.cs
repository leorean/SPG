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
        private float magic;
        private int maxMagic;

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
            magic = player.Magic;
            maxMagic = stats.MaxMagic;
        }

        public void Draw(SpriteBatch sb, GameTime gameTime)
        {
            var x = MainGame.Current.Camera.ViewX + 1;
            var y = MainGame.Current.Camera.ViewY + 1;

            // HP

            sb.Draw(Texture, new Vector2(x, y), new Rectangle(0, 0, 12, 12), Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 1);

            var hpFactor = 3;

            for(var i = 0; i < maxHP * hpFactor; i++)
            {
                var col = 0 + Convert.ToInt32(i < hp * hpFactor);
                
                sb.Draw(Texture, new Vector2(x + 12 + i, y), new Rectangle(16 + col, 0, 1, 7), Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 1);
            }

            // magic

            sb.Draw(Texture, new Vector2(x + 11, y + 7), new Rectangle(0, 16, 12, 12), Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 1);

            var magicFactor = .25f;

            for (var i = 0; i < maxMagic * magicFactor; i++)
            {
                var col = 0 + Convert.ToInt32(i < magic * magicFactor);

                sb.Draw(Texture, new Vector2(x + 11 + 12 + i, y + 7), new Rectangle(16 + col, 16, 1, 7), Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 1);
            }
        }

        
    }
}
