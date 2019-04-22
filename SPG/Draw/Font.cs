using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Map;
using System.Linq;
using System.Diagnostics;

namespace SPG.Draw
{
    public class Font
    {
        public enum HorizontalAlignment
        {
            Left, Center, Right
        }

        public enum VerticalAlignment
        {
            Top, Center, Bottom
        }

        // properties

        public HorizontalAlignment Halign = HorizontalAlignment.Center;
        public VerticalAlignment Valign = VerticalAlignment.Center;
        
        public float Depth { get; set; } = Globals.LAYER_FONT;

        public Color Color { get; set; } = Color.White;

        // private

        Dictionary<char, Texture2D> glyphs;

        // constructor

        public Font(TextureSet texture, int start, int spacing = 0)
        {
            glyphs = new Dictionary<char, Texture2D>();
            
            for (var i = 0; i < texture.Count; i++)
            {

                Debug.WriteLine($"Cropping {(char)(start + i)}...");
                var tex = CropFromMask(texture[i], spacing);

                glyphs.Add((char)(start + i), tex);                
            }

        }

        /// <summary>
        /// Takes a texture and calculates a width (+ spacing) based on the mask
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        private Texture2D CropFromMask(Texture2D texture, int spacing)
        {
            int max = 0;
            int min = texture.Width;

            Color mask = new Color(0, 0, 0, 0);

            Color[] pixels = texture.GetPixels();

            for(var i = 0; i <texture.Width; i++)
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

            var tex = texture.Crop(min, 0, max + spacing, texture.Height);

            return tex;
        }

        // draws single-line text

        public void Draw(float x, float y, string text)
        {
            var spriteBatch = GameManager.Game.SpriteBatch;
            
            var width = 0;
            var height = glyphs.FirstOrDefault().Value.Height;

            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                var tex = glyphs.Where(o => o.Key == c).FirstOrDefault().Value;
                width += tex.Width;
            }

            var posx = 0f;
            var posy = 0f;
            switch (Halign)
            {
                case HorizontalAlignment.Left:
                    posx = x;
                    break;
                case HorizontalAlignment.Center:
                    posx = x - .5f * width;
                    break;
                case HorizontalAlignment.Right:
                    posx = x - width;
                    break;
            }
            switch (Valign)
            {
                case VerticalAlignment.Top:
                    posy = y;
                    break;
                case VerticalAlignment.Center:
                    posy = y - .5f * height;
                    break;
                case VerticalAlignment.Bottom:
                    posy = y - height;
                    break;
            }

            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                var tex = glyphs.Where(o => o.Key == c).FirstOrDefault().Value;

                var pos = new Vector2(posx, posy);

                // we don't draw spaces
                if(c != ' ')
                    spriteBatch.Draw(tex, pos, null, Color, 0, Vector2.Zero, 1.0f, SpriteEffects.None, Depth);

                posx += tex.Width;
            }            
        }
    }
}
