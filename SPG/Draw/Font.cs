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

        public void Update()
        {
            // removes all texts that are not used anymore.
            texts.RemoveAll(t => t.DecreaseAliveCounter());
        }

        public void Draw(float x, float y, object text, int maxWidth = 0)
        {
            Draw(x, y, text.ToString(), maxWidth);
        }

        /// <summary>
        /// Draw text at a position, optionally limiting to a maxWidth in pixels.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="text"></param>
        /// <param name="maxWidth"></param>     
        public void Draw(float x, float y, string text, int maxWidth = 0)
        {
            //var sw = Stopwatch.StartNew();

            int lineHeight = glyphs.FirstOrDefault().Value.Height;
            var spriteBatch = GameManager.Game.SpriteBatch;
            
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
            else // re-use resources
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
                            posx = x - .5f * textObject.LineTextures[i].Width;
                            break;
                        case HorizontalAlignment.Right:
                            posx = x - textObject.LineTextures[i].Width;
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
                    spriteBatch.Draw(textObject.LineTextures[i], pos, null, Color, 0, Vector2.Zero, 1.0f, SpriteEffects.None, Depth);
                }
            }

            //Debug.WriteLine("Drawing text took " + sw.ElapsedMilliseconds + "ms");
            //sw.Stop();            
        }        
    }
}
