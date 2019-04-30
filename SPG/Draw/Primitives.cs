using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Util;
using System;

namespace SPG.Draw
{
    static class Primitives2D
    {
        private static Texture2D pixel; //our pixel texture we will be using to draw primitives

        private static bool initialized = false;

        private static void Setup(GraphicsDevice gd)
        {
            if (initialized) return;

            //creating our simple pixel
            pixel = new Texture2D(gd, 1, 1);
            pixel.SetData(new Color[] { Color.White });

            initialized = true;
        }

        static Primitives2D()
        {
        }

        //draws a pixel
        public static void DrawPixel(this SpriteBatch sb, float x, float y, Color col)
        {
            Setup(sb.GraphicsDevice);
            sb.Draw(pixel, new Vector2(x, y), null, col, 0, Vector2.Zero, 1f, SpriteEffects.None, 1);
        }

        //draws a line 
        public static void DrawLine(this SpriteBatch sb, float x1, float y1, float x2, float y2, Color col)
        {
            float deltax, deltay, x, y, xinc1, xinc2, yinc1, yinc2, den, num, numadd, numpixels, curpixel;
            deltax = Math.Abs(x2 - x1);        // The difference between the x's
            deltay = Math.Abs(y2 - y1);        // The difference between the y's
            x = x1;                       // Start x off at the first pixel
            y = y1;                       // Start y off at the first pixel

            if (x2 >= x1)                 // The x-values are increasing
            {
                xinc1 = 1;
                xinc2 = 1;
            }
            else                          // The x-values are decreasing
            {
                xinc1 = -1;
                xinc2 = -1;
            }

            if (y2 >= y1)                 // The y-values are increasing
            {
                yinc1 = 1;
                yinc2 = 1;
            }
            else                          // The y-values are decreasing
            {
                yinc1 = -1;
                yinc2 = -1;
            }

            if (deltax >= deltay)         // There is at least one x-value for every y-value
            {
                xinc1 = 0;                  // Don't change the x when numerator >= denominator
                yinc2 = 0;                  // Don't change the y for every iteration
                den = deltax;
                num = deltax / 2;
                numadd = deltay;
                numpixels = deltax;         // There are more x-values than y-values
            }
            else                          // There is at least one y-value for every x-value
            {
                xinc2 = 0;                  // Don't change the x for every iteration
                yinc1 = 0;                  // Don't change the y when numerator >= denominator
                den = deltay;
                num = deltay / 2;
                numadd = deltax;
                numpixels = deltay;         // There are more y-values than x-values
            }

            for (curpixel = 0; curpixel <= numpixels; curpixel++)
            {
                DrawPixel(sb, x, y, col);
                num += numadd;              // Increase the numerator by the top of the fraction
                if (num >= den)             // Check if numerator >= denominator
                {
                    num -= den;               // Calculate the new numerator value
                    x += xinc1;               // Change the x as appropriate
                    y += yinc1;               // Change the y as appropriate
                }
                x += xinc2;                 // Change the x as appropriate
                y += yinc2;                 // Change the y as appropriate
            }
        }

        public static void DrawRectangle(this SpriteBatch sb, RectF rect, Color col, Boolean filled)
        {
            if (filled)
            {
                sb.Draw(pixel, new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height), col);
            }
            else
            {
                DrawLine(sb, rect.X, rect.Y, rect.X + rect.Width, rect.Y, col);
                DrawLine(sb, rect.X, rect.Y, rect.X, rect.Y + rect.Height, col);
                DrawLine(sb, rect.X + rect.Width, rect.Y, rect.X + rect.Width, rect.Y + rect.Height, col);
                DrawLine(sb, rect.X, rect.Y + rect.Height, rect.X + rect.Width, rect.Y + rect.Height, col);
            }
        }
    }
}