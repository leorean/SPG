using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Draw;
using System;

namespace Leore.Util
{
    public static class DrawExtensions
    {
        public static void DrawBar(this SpriteBatch sb, Vector2 position, int width, float value, Color fg, Color bg, float depth = 1.0f, int height = 4, bool border = true)
        {
            var x = position.X - width * .5f;
            var y = position.Y;

            height--;

            for(var i = 0; i < width; i++)
            {
                // border:

                if (i == 0 || i == width - 1)
                {
                    if (border)
                    {
                        sb.DrawLine(x + i, y + 1, x + i, y + height - 1, Color.White, depth);
                    }
                }
                else
                {
                    if (border)
                    {
                        sb.DrawPixel(x + i, y, Color.White, depth);
                        sb.DrawPixel(x + i, y + height, Color.White, depth);                        
                    }

                    var col = Math.Floor(value * width) > i ? fg : bg;
                    sb.DrawLine(x + i, y + 1 * Convert.ToInt32(border), x + i, y + height - 1 * Convert.ToInt32(border), col, depth);
                }                
            }

        }
    }
}
