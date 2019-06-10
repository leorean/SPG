using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Draw;
using SPG.Util;
using System;
using System.Collections.Generic;

namespace Leore.Util
{
    public static class DrawExtensions
    {
        public static void DrawBar(this SpriteBatch sb, Vector2 position, int width, float value, Color fg, Color bg, float depth = 1.0f, int height = 4, bool border = true, Color? borderColor = null)
        {
            if (borderColor == null) borderColor = Color.White;

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
                        sb.DrawLine(x + i, y + 0, x + i, y + height - 0, (Color)borderColor, depth);
                    }
                }
                else
                {
                    if (border)
                    {
                        sb.DrawPixel(x + i, y, (Color)borderColor, depth);
                        sb.DrawPixel(x + i, y + height, (Color)borderColor, depth);                        
                    }

                    var col = Math.Floor(value * width) > i ? fg : bg;
                    sb.DrawLine(x + i, y + 1 * Convert.ToInt32(border), x + i, y + height - 1 * Convert.ToInt32(border), col, depth);
                }                
            }
        }

        public static void DrawLightning(this SpriteBatch sb, Vector2 a, Vector2 b, Color color, float depth)
        {

            var steps = Math.Max(MathUtil.Div(Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y)), 4), 3);
            
            var p = new float[steps + 1];
            var q = new float[steps + 1];

            p[0] = a.X;
            q[0] = a.Y;
            p[steps] = b.X;
            q[steps] = b.Y;

            var ca = color.A / 255f;
            float alpha = 0;

            for(var i = 1; i < steps; i++)
            {
                alpha = (float)Math.Sin((i / (float)steps) * Math.PI) * ca;

                p[i] = p[0] + (i / (float)steps) * (p[steps] - p[0]) - 2 + (float)RND.Next * 4;
                q[i] = q[0] + (i / (float)steps) * (q[steps] - q[0]) - 2 + (float)RND.Next * 4;

                sb.DrawLine(p[i - 1], q[i - 1], p[i], q[i], new Color(color, alpha), depth);
            }
            sb.DrawLine(p[steps - 1], q[steps - 1], p[steps], q[steps], new Color(color, alpha), depth);            
        }
    }
}
