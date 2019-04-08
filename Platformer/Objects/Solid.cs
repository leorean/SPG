using Microsoft.Xna.Framework;
using SPG.Objects;
using System;

namespace Platformer
{
    public class Solid : GameObject
    {
        public Solid(int x, int y)
        {
            Name = "solid";
            Position = new Vector2(x, y);
            BoundingBox = new SPG.Util.RectF(0, 0, Globals.TILE, Globals.TILE);
        }
    }
}