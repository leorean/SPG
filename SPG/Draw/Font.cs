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
    public static class StringExtensions
    {
        public static List<int> AllIndexesOf(this string str, string value)
        {
            List<int> indices = new List<int>();

            if (String.IsNullOrEmpty(value))
                return indices;
            
            for (int i = 0; ; i += value.Length)
            {
                i = str.IndexOf(value, i);
                if (i == -1)
                    return indices;
                indices.Add(i);
            }
        }
    }

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

        /// <summary>
        /// Nested class to store information about texts for reusing resources
        /// </summary>
        class Text
        {
            public List<Texture2D> LineTextures { get; set; }
            public string Content { get; set; }

            private int aliveCounter { get; set; }

            public Text(string text)
            {
                LineTextures = new List<Texture2D>();
                Content = text;
                ResetAliveCounter();
            }

            public void ResetAliveCounter()
            {
                aliveCounter = 10;
            }

            public bool DecreaseAliveCounter()
            {
                aliveCounter = Math.Max(aliveCounter - 1, 0);

                return aliveCounter == 0;
            }
        }

        // properties

        public HorizontalAlignment Halign = HorizontalAlignment.Center;
        public VerticalAlignment Valign = VerticalAlignment.Center;
        
        public float Depth { get; set; } = Globals.LAYER_FONT;
        public Color Color { get; set; } = Color.White;
        public Color HighlightColor { get; set; } = Color.LimeGreen;
        public uint Spacing { get; set; }
        // private

        private Dictionary<char, Texture2D> glyphs;
        private List<Text> texts = new List<Text>();
        
        // constructor

        /// <summary>
        /// Creates a new font, based on a texture set and with a set starting character index
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="startCharacter"></param>
        /// <param name="spacing"></param>
        public Font(TextureSet texture, int startCharacter, uint spacing = 1)
        {
            glyphs = new Dictionary<char, Texture2D>();

            Spacing = spacing;

            for (var i = 0; i < texture.Count; i++)
            {
                var tex = texture[i].CropFromBackgroundColor(0);
                glyphs.Add((char)(startCharacter + i), tex);
            }

            var spaceTex = glyphs.Where(o => o.Key == ' ').FirstOrDefault().Value;

            if (spaceTex != null)
            {
                glyphs[' '] = spaceTex.Clear();
            }
        }

        private Font() { }

        // methods

        public Font Copy()
        {
            return (Font)MemberwiseClone();            
        }

        public void Update()
        {
            // removes all texts that are not used anymore.
            texts.RemoveAll(t => t.DecreaseAliveCounter());
        }

        public int GetWidth(string text)
        {
            var lines = text.ToString().Split('\n');

            for (var l = 0; l < lines.Length; l++)
            {
                var txt = lines[l];

                int totalLineWidth = 0;

                bool cont = false;

                // necessary pre-calc

                for (var i = 0; i < txt.Length; i++)
                {
                    var c = txt[i];
                    var tex = glyphs.Where(o => o.Key == c).FirstOrDefault().Value;

                    if (c == ']' && cont)
                    {
                        cont = false;
                    }

                    if (c == '~' || cont)
                        continue;

                    if (c == '[')
                    {
                        cont = true;
                    }

                    // draw first texture when glyph is not found in set.
                    if (c != '\n' && tex == null)
                        tex = glyphs.FirstOrDefault().Value;

                    totalLineWidth += tex.Width + (int)Spacing;
                }
                return totalLineWidth;
            }
            
            return 0;
        }

        /// <summary>
        /// Draw text at a position, optionally limiting to a maxWidth in pixels.
        /// Special chars:
        /// \n ... new line
        /// ~ ... toggle highlighting (example: ~word~)
        /// [ffffff] ... overwrite hightlight color
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="text"></param>
        /// <param name="maxWidth"></param>     
        public void Draw(SpriteBatch sb, float x, float y, object text, int maxWidth = 0, float scale = 1f, float? depth = null)
        {
            Color hiColor = HighlightColor;

            int lineHeight = (int)(glyphs.FirstOrDefault().Value.Height * scale);

            var lines = text.ToString().Split('\n');

            for (var l = 0; l < lines.Length; l++)
            {
                bool highLight = false;

                var txt = lines[l];

                float lineWidth = 0;
                float totalLineWidth = 0;

                bool cont = false;

                // necessary pre-calc
                
                for (var i = 0; i < txt.Length; i++)
                {
                    var c = txt[i];
                    var tex = glyphs.Where(o => o.Key == c).FirstOrDefault().Value;

                    if (c == ']' && cont)
                    {
                        cont = false;
                    }

                    if (c == '~' || cont)
                        continue;
                    
                    if (c == '[')
                    {
                        cont = true;
                    }

                    // draw first texture when glyph is not found in set.
                    if (c != '\n' && tex == null)
                        tex = glyphs.FirstOrDefault().Value;

                    totalLineWidth += tex.Width + (int)Spacing;
                }

                // draw

                for (var i = 0; i < txt.Length; i++)
                {
                    var c = txt[i];
                    var tex = glyphs.Where(o => o.Key == c).FirstOrDefault().Value;

                    if (c == '~')
                    {
                        highLight = !highLight;

                        if (highLight == false)
                        {
                            hiColor = HighlightColor; // resets overridden color
                        }

                        continue;
                    }

                    if (c == '[')
                    {
                        var hexCode = "";
                        while (c != ']')
                        {
                            if (i == txt.Length - 1)
                                break;

                            i++;
                            c = txt[i];
                            if (c != ']')
                                hexCode += c;
                        }
                        if (c != ']')
                            continue;

                        hiColor = Colors.FromHex(hexCode);
                        continue;
                    }
                    
                    var color = highLight ? hiColor : Color;

                    // draw first texture when glyph is not found in set.
                    if (c != '\n' && tex == null)
                        tex = glyphs.FirstOrDefault().Value;
                    
                    var posx = x;
                    var posy = y;
                    
                    switch (Valign)
                    {
                        case VerticalAlignment.Top:
                            posy = y;
                            break;
                        case VerticalAlignment.Center:
                            posy = y - .5f * lineHeight * lines.Length;
                            break;
                        case VerticalAlignment.Bottom:
                            posy = y - lineHeight * lines.Length;
                            break;
                    }

                    switch (Halign)
                    {
                        case HorizontalAlignment.Left:
                            posx = x;
                            break;
                        case HorizontalAlignment.Center:
                            posx = x - .5f * totalLineWidth * scale;                            
                            break;
                        case HorizontalAlignment.Right:                            
                            posx = x - totalLineWidth * scale;                            
                            break;
                    }

                    var pos = new Vector2(posx + lineWidth, posy + l * lineHeight);
                    sb.Draw(tex, pos, null, color, 0, Vector2.Zero, scale, SpriteEffects.None, (depth == null) ? Depth : (float)depth);
                    lineWidth += (tex.Width + (int)Spacing) * scale;

                }                
            }
        }        
    }
}
