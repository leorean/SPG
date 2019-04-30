using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace SPG.Draw
{
    public static class DrawExtensions
    {
        /// <summary>
        /// Returns an empty copy of the texture.
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        public static Texture2D Clear(this Texture2D texture)
        {
            var tex = new Texture2D(texture.GraphicsDevice, texture.Width, texture.Height);
            
            Color[] data = new Color[tex.Width * tex.Height];

            for (int i = 0; i < data.Length; i++)
            {
                data[i] = new Color(0, 0, 0, 0);
            }
            
            tex.SetData(data);            
            return tex;
        }

        /// <summary>
        /// Takes a texture and calculates a width (+ spacing) based on the mask. Height remains unaltered.
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        public static Texture2D CropFromBackgroundColor(this Texture2D texture, int spacingRight)
        {
            int max = 0;
            int min = texture.Width;

            Color mask = new Color(0, 0, 0, 0);

            Color[] pixels = texture.GetPixels();

            for (var i = 0; i < texture.Width; i++)
            {
                for (var j = 0; j < texture.Height; j++)
                {
                    var px = pixels.GetPixel(i, j, texture.Width);

                    if (px != mask)
                    {
                        max = Math.Max(i, max);
                        min = Math.Min(i, min);
                    }
                }
            }
            
            var tex = texture.Crop(min, max + spacingRight);

            return tex;
        }

        /// <summary>
        /// Appends a texture on the right side of the current one. Height must be equal.
        /// </summary>
        /// <param name="tex"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Texture2D AppendRight(this Texture2D tex, Texture2D other)
        {
            if (tex.Height != other.Height)
                throw new ArgumentException("The height of both textures must be equal!");

            var rect = new Rectangle(0, 0, tex.Width + other.Width, tex.Height);

            var newTexture = new Texture2D(GameManager.Game.GraphicsDevice, rect.Width, rect.Height);

            Color[] data = new Color[rect.Width * rect.Height];
            
            Color[] left = tex.GetPixels();
            Color[] right = other.GetPixels();

            var leftGrid = new Grid<Color>(tex.Width, tex.Height);
            for (var i = 0; i < left.Length; i++)
                leftGrid.Set(i, left[i]);
            var rightGrid = new Grid<Color>(other.Width, other.Height);
            for (var i = 0; i < right.Length; i++)
                rightGrid.Set(i, right[i]);

            var totalGrid = new Grid<Color>(rect.Width, rect.Height);
            
            for(var j = 0; j < rect.Height; j++)
            {
                for (var i = 0; i < rect.Width; i++)
                {
                    Color val;
                    if (i < tex.Width)
                        val = leftGrid.Get(i, j);
                    else
                        val = rightGrid.Get(i - tex.Width, j);

                    totalGrid.Set(i, j, val);                    
                }
            }

            data = totalGrid.ToList().ToArray();

            newTexture.SetData(data);
            return newTexture;
        }

        public static Texture2D Crop(this Texture2D tex, int left, int right)
        {
            if (left >= right)
                return tex;
            var width = (right - left) + 1;

            // create a transparent row

            Texture2D row = new Texture2D(tex.GraphicsDevice, 1, tex.Height);
            var rowColors = new Color[tex.Height];
            for (var i = 0; i < rowColors.Length; i++)
                rowColors[i] = new Color(0, 0, 0, 0);
            row.SetData(rowColors);

            // append that row to the texture on the right side
            while(tex.Width < (left + right) + 1)
                tex = tex.AppendRight(row);
                        
            var rect = new Rectangle(left, 0, width, tex.Height);
            var newTexture = new Texture2D(GameManager.Game.GraphicsDevice, rect.Width, rect.Height);

            Color[] data = new Color[rect.Width * rect.Height];
            tex.GetData(0, rect, data, 0, rect.Width * rect.Height);
            newTexture.SetData(data);

            return newTexture;
        }

        public static Color GetPixel(this Color[] colors, int x, int y, int width)
        {
            return colors[x + (y * width)];
        }
        public static Color[] GetPixels(this Texture2D texture)
        {
            Color[] colors1D = new Color[texture.Width * texture.Height];
            texture.GetData(colors1D);
            return colors1D;
        }
    }
}
