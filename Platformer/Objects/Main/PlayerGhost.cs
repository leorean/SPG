using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Platformer.Main;
using SPG.Objects;
using SPG.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects.Main
{
    public class PlayerGhost : GameObject
    {
        private double alpha = .01;
        private float spawnY;

        public PlayerGhost(float x, float y, Direction dir, string name = null) : base(x, y, name)
        {
            DrawOffset = new Vector2(8, 24);
            BoundingBox = new RectF(-4, -4, 8, 12);

            Texture = AssetManager.PlayerGhostSprite;

            Scale = new Vector2(Math.Sign((int)dir), 1);
            spawnY = Y;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Move(0, -.7f);
            
            Color = new Color(Color, (float)alpha);

            if (Math.Abs(Y - spawnY) < 3 * Globals.TILE)
            {
                alpha = Math.Min(alpha + .01, .5);

            } else
            {
                alpha = Math.Max(alpha - .005, 0);

                if (alpha == 0)
                {
                    Parent = null;
                    Destroy();
                    GameManager.Current.ReloadLevel();
                }
            }
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {            
            base.Draw(sb, gameTime);
        }
    }
}
