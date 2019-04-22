using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace SPG.Draw
{
    public static class DrawExtensions
    {

        public static Texture2D Crop(this Texture2D tex, int x, int y, int w, int h)
        {
            if (x + w > tex.Width)
                w = tex.Width - x;
            if (y + h > tex.Height)
                h = tex.Height - y;

            if (w == 0 || h == 0)
                return tex;

            var rect = new Rectangle(x, y, w, h);
            var newTexture = new Texture2D(GameManager.Game.GraphicsDevice, rect.Width, rect.Height);

            Color[] data = new Color[rect.Width * rect.Height];

            tex.GetData(0, rect, data, 0, data.Length);
            newTexture.SetData(data);

            return newTexture;

            /*Rectangle sourceRectangle = new Rectangle(i * (int)tileWidth, j * (int)tileHeight, (int)tileWidth, (int)tileHeight);
                        var newTexture = new Texture2D(GameManager.Game.GraphicsDeviceManager.GraphicsDevice, sourceRectangle.Width, sourceRectangle.Height);
                        Color[] data = new Color[sourceRectangle.Width * sourceRectangle.Height];

                        texture.GetData(0, sourceRectangle, data, 0, data.Length);
                        newTexture.SetData(data);
            */


        }

        public static Color GetPixel(this Color[] colors, int x, int y, int width)
        {
            return colors[x + (y * width)];
        }
        public static Color[] GetPixels(this Texture2D texture)
        {
            Color[] colors1D = new Color[texture.Width * texture.Height];
            texture.GetData<Color>(colors1D);
            return colors1D;
        }
    }
}
