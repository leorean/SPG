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

        /// <summary>
        /// Creates a new font, based on a texture set and with a set starting character index
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="startCharacter"></param>
        /// <param name="spacing"></param>
        public Font(TextureSet texture, int startCharacter, int spacing = 0)
        {
            glyphs = new Dictionary<char, Texture2D>();
            
            for (var i = 0; i < texture.Count; i++)
            {
                var tex = texture[i].CropFromBackgroundColor(spacing);
                glyphs.Add((char)(startCharacter + i), tex);
            }

            var spaceTex = glyphs.Where(o => o.Key == ' ').FirstOrDefault().Value;

            if (spaceTex != null)
            {
                glyphs[' '] = spaceTex.Clear();
            }            
        }
        
        // draws text

        public void Draw(float x, float y, string text, int maxWidth = 0)
        {
            var spriteBatch = GameManager.Game.SpriteBatch;

            var line = text.Split('\n');

            List<Texture2D> lineTextures = new List<Texture2D>();

            int textHeight = glyphs.FirstOrDefault().Value.Height;

            // prepare
            for (var l = 0; l < line.Length; l++)
            {
                var txt = line[l];
                Texture2D word = null;
                for (var i = 0; i < txt.Length; i++)
                {
                    var c = txt[i];
                    var tex = glyphs.Where(o => o.Key == c).FirstOrDefault().Value;

                    // draw ? when glyph is not found in set.
                    if (c != '\n' && tex == null)
                        tex = glyphs.Where(o => o.Key == '?').FirstOrDefault().Value;
                    
                    if (maxWidth > 0 && word != null && word.Width + tex.Width > maxWidth)
                    {
                        lineTextures.Add(word);                        
                        word = tex;
                        continue;
                    }

                    word = (word == null) ? tex : word.AppendRight(tex);
                }
                // add line
                lineTextures.Add(word);                
            }

            var posx = x;
            switch (Halign)
            {
                case HorizontalAlignment.Left:
                    posx = x;
                    break;
                case HorizontalAlignment.Center:
                    posx = x - .5f * lineTextures.Max(o => o.Width);
                    break;
                case HorizontalAlignment.Right:
                    posx = x + lineTextures.Max(o => o.Width);
                    break;
            }

            // draw lines
            for (var i = 0; i < lineTextures.Count; i++)
            {
                var pos = new Vector2(x, y + i * textHeight);
                spriteBatch.Draw(lineTextures[i], pos, null, Color, 0, Vector2.Zero, 1.0f, SpriteEffects.None, Depth);
            }
        }

        /*
        public void Draw(float x, float y, string text, int maxWidth = 0)
        {
            var spriteBatch = GameManager.Game.SpriteBatch;

            List<int> lineWidths = new List<int>();
            int line = 0;

            lineWidths.Add(0);

            var charHeight = glyphs.FirstOrDefault().Value.Height;
            
            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                var tex = glyphs.Where(o => o.Key == c).FirstOrDefault().Value;

                // draw ? when glyph is not found in set.
                if (c != '\n' && tex == null)
                    tex = glyphs.Where(o => o.Key == '?').FirstOrDefault().Value;
                
                if (c == '\n' || (lineWidths[line] + tex.Width > maxWidth && maxWidth > 0))
                {
                    // ignore special case where first char is newline..
                    if (!(lineWidths[line] == 0 && c == '\n'))
                    {
                        line++;
                        lineWidths.Add(0);
                    }                    
                } else
                {
                    lineWidths[line] += tex.Width;
                }                
            }

            var maxLineWidth = lineWidths.Max();            

            var posx = 0f;
            var posy = 0f;
            switch (Halign)
            {
                case HorizontalAlignment.Left:
                    posx = x;
                    break;
                case HorizontalAlignment.Center:
                    posx = x - .5f * maxLineWidth;
                    break;
                case HorizontalAlignment.Right:
                    posx = x - maxLineWidth;
                    break;
            }
            switch (Valign)
            {
                case VerticalAlignment.Top:
                    posy = y;
                    break;
                case VerticalAlignment.Center:
                    posy = y - .5f * charHeight * lineWidths.Count;
                    break;
                case VerticalAlignment.Bottom:
                    posy = y - charHeight * lineWidths.Count;
                    break;
            }

            var tx = posx;
            var ty = posy;

            //var lastX = posx + maxLineWidth;
            line = 0; // reset

            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                var tex = glyphs.Where(o => o.Key == c).FirstOrDefault().Value;

                // draw ? when glyph is not found in set.
                if (c != '\n' && tex == null)
                    tex = glyphs.Where(o => o.Key == '?').FirstOrDefault().Value;

                var pos = new Vector2(tx, ty);

                // we don't draw spaces
                if (c != ' ' && c != '\n')
                    spriteBatch.Draw(tex, pos, null, Color, 0, Vector2.Zero, 1.0f, SpriteEffects.None, Depth);

                if (c == '\n' || ((tx - posx) + tex.Width > maxWidth && maxWidth > 0))
                {
                    // ignore special case where first char is newline..
                    if (!(tx == posx && c == '\n'))
                    {
                        ty += charHeight;                        
                    }
                    tx = posx;
                }
                else
                {
                    tx += tex.Width;
                }                
            }
        }
        */
    }
}
