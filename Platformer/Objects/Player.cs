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
        }

        public override void Update()
        {
            base.Update();

            var candidates = ObjectManager.Find(this, X, Y, typeof(Solid));

        }

        public override void Draw()
        {
            base.Draw();
        }
    }
}