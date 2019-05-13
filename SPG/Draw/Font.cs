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
                aliveCounter = 30;
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
        public Font(TextureSet texture, int startCharacter, int spacing = 1)
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

        private Font() { }

        // methods

        public Font Copy()
        {
            var copy = new Font();

            copy.Color = Color;
            copy.Depth = Depth;
            copy.glyphs = glyphs;
            copy.Halign = Halign;
            copy.Valign = Valign;
            
            return copy;
        }

        public void Update()
        {
            // removes all texts that are not used anymore.
            texts.RemoveAll(t => t.DecreaseAliveCounter());
        }

        /// <summary>
        /// Draw text at a position, optionally limiting to a maxWidth in pixels.
        /// Special chars:
        /// \n ... new line
        /// ~ ... toggle highlighting (example: ~word~)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="text"></param>
        /// <param name="maxWidth"></param>     
        public void Draw(SpriteBatch sb, float x, float y, object text, int maxWidth = 0, float scale = 1f, float? depth = null)
        {
            Draw(sb, x, y, text.ToString(), maxWidth, scale, depth);
        }
        
        internal void Draw(SpriteBatch sb, float x, float y, string text, int maxWidth = 0, float scale = 1f, float? depth = null)
        {
            //var sw = Stopwatch.StartNew();

            int lineHeight = (int)(glyphs.FirstOrDefault().Value.Height * scale);
            
            // removes all texts that are not used anymore.
            texts.RemoveAll(t => t.DecreaseAliveCounter());

            // find old resource if possible and re-use

            var textObject = texts.Where(o => o.Content == text).FirstOrDefault();

            if (textObject == null)
            {
                textObject = new Text(text);
                texts.Add(textObject);
                
                var line = text.Split('\n');
                
                textObject.LineTextures = new List<Texture2D>();
                
                // prepare
                for (var l = 0; l < line.Length; l++)
                {
                    bool highLight = false;

                    var txt = line[l];
                    
                    Texture2D word = null;
                    for (var i = 0; i < txt.Length; i++)
                    {
                        var c = txt[i];
                        var tex = glyphs.Where(o => o.Key == c).FirstOrDefault().Value;
                        
                        if (c == '~')
                        {
                            highLight = !highLight;
                            continue;
                        }

                        if (highLight)
                            tex = tex.ReplaceColor(Color, HighlightColor);

                        // draw first texture when glyph is not found in set.
                        if (c != '\n' && tex == null)
                            tex = glyphs.FirstOrDefault().Value;

                        if (maxWidth > 0 && word != null && word.Width * scale + tex.Width * scale > maxWidth * scale)
                        {
                            textObject.LineTextures.Add(word);
                            word = tex;
                            continue;
                        }

                        word = (word == null) ? tex : word.AppendRight(tex);
                    }
                    // add line
                    textObject.LineTextures.Add(word);
                }
            }
            // re-use resources
            {
                textObject.ResetAliveCounter();

                // draw text - line by line
                for (var i = 0; i < textObject.LineTextures.Count; i++)
                {
                    var posx = x;
                    var posy = y;
                    switch (Halign)
                    {
                        case HorizontalAlignment.Left:
                            posx = x;
                            break;
                        case HorizontalAlignment.Center:
                            posx = x - .5f * (textObject.LineTextures[i] != null ? textObject.LineTextures[i].Width : 0) * scale;
                            break;
                        case HorizontalAlignment.Right:
                            posx = x - (textObject.LineTextures[i] != null ? textObject.LineTextures[i].Width : 0) * scale;
                            break;
                    }

                    switch (Valign)
                    {
                        case VerticalAlignment.Top:
                            posy = y;
                            break;
                        case VerticalAlignment.Center:
                            posy = y - .5f * lineHeight * textObject.LineTextures.Count;
                            break;
                        case VerticalAlignment.Bottom:
                            posy = y - lineHeight * textObject.LineTextures.Count;
                            break;
                    }

                    var pos = new Vector2(posx, posy + i * lineHeight);
                    if (textObject.LineTextures[i] != null)
                        sb.Draw(textObject.LineTextures[i], pos, null, Color, 0, Vector2.Zero, scale, SpriteEffects.None, (depth == null) ? Depth : (float)depth);
                }
            }       
        }
    }
}
