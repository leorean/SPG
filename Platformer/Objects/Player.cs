using Microsoft.Xna.Framework;
using SPG.Objects;
using System;

namespace Platformer
{
    public class Player : GameObject
    {
        public Player(int x, int y)
        {
            Name = "Player";
            Position = new Vector2(x, y);
            BoundingBox = new SPG.Util.RectF(0, 0, Globals.TILE, Globals.TILE);

            Gravity = .1f;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            YVel += Gravity;

            var colY = ObjectManager.Find(this, X, Y + YVel, typeof(Solid));
            if (colY.Count == 0)
            {
                Position += new Vector2(0, YVel);
            } else
            {
                YVel = 0;
            }

            var colX = ObjectManager.Find(this, X + XVel, Y, typeof(Solid));

            if (colX.Count == 0)
            {
                Position += new Vector2(XVel, 0);
            } else
            {
                XVel = 0;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }
}