using Microsoft.Xna.Framework;
using Platformer.Objects.Effects;
using Platformer.Objects.Main;
using SPG.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects.Items
{
    public static class CoinExtensions
    {
        public static Coin.CoinValue TileIDToCoinValue(this int i)
        {
            if (i == 0) return Coin.CoinValue.V1;
            if (i == 1) return Coin.CoinValue.V2;
            if (i == 2) return Coin.CoinValue.V3;
            if (i == 3) return Coin.CoinValue.V4;
            if (i == 4) return Coin.CoinValue.V5;
            if (i == 5) return Coin.CoinValue.V6;
            if (i == 6) return Coin.CoinValue.V7;

            throw new ArgumentException("Unable to parse tile ID to coin value!");
        }
    }

    public class Coin : Item
    {
        public enum CoinValue
        {
            V1 = 1, V2 = 2, V3 = 5, V4 = 10,
            V5 = 50, V6 = 100,
            V7 = 200
        }

        public CoinValue Value { get; set; }

        private double t;
        private double sin;
        private double alpha = 2;
        private Vector2 pos;

        //private bool initialized;

        // WARNING: currently, it is not possible to spawn coins from anywhere and guarantee save persistance!!

        public Coin(float x, float y, Room room, string name = null) : base(x, y, room, name)
        {
            Visible = false;
            AnimationTexture = AssetManager.Coins;
            t = RND.Next * Math.PI * 2;
            pos = Position;

            Save = true;
            Respawn = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            /*if (!Taken && GameManager.Current.Player.Stats.CollectedCoins.ContainsKey(ID))
            {
                Destroy();
                return;
            } else
            {
                Visible = true;
            }
            initialized = true;*/

            if (!Taken)
            {
                t = (t + .06) % (2 * Math.PI);
                sin = Math.Sin(t) * 1.5;
                Position = new Vector2(pos.X, pos.Y + (float)sin);
            }
            else
            {
                sin = 0;
                Move(0, -1f);
                alpha = Math.Max(alpha - .1, 0);

                Color = new Color(Color, (float)alpha);
                
                if (alpha == 0)
                {
                    //var eff = new SingularEffect(X, Y, 1);                    
                    Destroy();
                }
            }
            
            // draw logic

            int row = 0;
            int cols = 4;
            float fSpd = .13f;
            switch (Value)
            {
                case CoinValue.V1:
                    row = 0;
                    break;
                case CoinValue.V2:
                    row = 1;
                    break;
                case CoinValue.V3:
                    row = 2;
                    break;
                case CoinValue.V4:
                    row = 3;
                    break;
                case CoinValue.V5:
                    row = 4;
                    break;
                case CoinValue.V6:
                    row = 5;
                    break;
                case CoinValue.V7:
                    row = 6;
                    break;                
            }
            SetAnimation(cols * row, cols * row + cols - 1, fSpd, true);
        }

        public override void Take(Player player)
        {
            if (!initialized)
                return;
            if (!Taken)
            {
                player.Stats.Coins += (int)Value;
                player.CoinCounter += (int)Value;
                Taken = true;
            }
            //throw new NotImplementedException();
        }
    }
}
