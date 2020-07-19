using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SPG;
using SPG.Draw;
using SPG.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Leore.Main
{
    public class MessageBox : GameObject, IKeepEnabledAcrossRooms
    {
        public enum TextSpeed
        {
            NORMAL = 3,
            SLOW = 10
        }

        public int AppearDelay { get; set; }

        private int soundTimeout;

        private TextSpeed textSpeed;

        public Action OnCompleted;

        public int CompleteActionDelay;

        protected List<string> texts;
        protected Font font;
        protected int maxWidth;
        protected float zoom;

        protected int offY = 6 * Globals.T;
        protected int page = 0;
        protected string curText = "";
        protected float sin = 0;
        
        private int timeOut;
        private Color? hiColor;
        protected Font.HorizontalAlignment halign;
        protected Font.VerticalAlignment valign;

        protected bool kLeftPressed, kRightPressed, kPrevPressed, kNextPressed;
        protected float alpha;

        protected bool showBorder;

        protected int option = 1; // just used for dialog

        public enum MessageState { FADE_IN, SHOW, FADE_OUT }
        protected MessageState state;

        float cursorAlpha = 0;
        
        private List<string> CutString(string str, int maxWidth)
        {
            List<string> list = new List<string>();

            str = str.Replace("\n", " ");

            var words = str.Split(' ');

            var line = "";

            foreach(var word in words)
            {
                var lineWidth = font.GetWidth(line);
                var wordWidth = font.GetWidth(" " + word);
                
                if (wordWidth > maxWidth)
                {
                    throw new ArgumentException($"The text {word} exceeds the maximum pixel width! Max. allowed width is {maxWidth} - the text was {wordWidth} pixels wide!", "text");
                }

                if (lineWidth + wordWidth > maxWidth)
                {
                    list.Add(line);
                    line = word;                    
                }
                else
                {
                    line += " " + word;
                }
            }
            list.Add(line);

            // cleans "~" highlights

            int i = 0;
            bool hadHighlight = false;
            foreach(var l in list.ToList())
            {
                if (hadHighlight)
                {
                    list[i] = "~" + l;

                    if (list[i].Count(f => f == '~') == 2)
                    {
                        hadHighlight = false;
                    }
                }
                else
                {
                    if (l.Count(f => f == '~') % 2 == 1)
                    {
                        list[i] = l + "~";
                        hadHighlight = true;
                    }
                }
                i++;
            }
            
            return list;
        }

        /// <summary>
        /// Creates a new message box.
        /// 
        /// Special chars:
        /// \n ... new line
        /// | ... new page
        /// ~ ... toggle highlighting (example: ~word~)
        /// [ffffff] ... overriding the highlight color
        /// 
        /// #l ... left arrow
        /// #r ... right arrow
        /// #u ... up arrow
        /// #d ... down arrow
        /// 
        /// #a ... A button
        /// #b ... B button
        /// #x ... X button
        /// #y ... Y button
        /// #A ... A key
        /// #S ... S key
        /// #lu ... left-up
        /// #ld ... left-down
        /// #ru ... right-up
        /// #rd ... right-down
        ///
        /// #D ... D key
        /// #lt ... left-trigger
        /// #rt ... right-trigger
        /// #Q ... Q
        /// #E ... E
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="text"></param>
        /// <param name="name"></param>
        public MessageBox(string text, bool centerText = false, string name = null, Color? hiColor = null, TextSpeed textSpeed = TextSpeed.NORMAL, bool showBorder = true) : base(0, 0, name)
        {
            if (Properties.Settings.Default.HighResText == true)
            {
                font = AssetManager.MessageFont.Copy();
                zoom = .5f;
                font.Spacing = (uint)(1 / zoom);
            }
            else
            {
                font = AssetManager.DefaultFont.Copy();
                zoom = 1f;
                font.Spacing = 1;
            }

            halign = centerText ? Font.HorizontalAlignment.Center : Font.HorizontalAlignment.Left;
            valign = Font.VerticalAlignment.Top;

            texts = new List<string>();

            text = text.Replace("#lu", ((char)138).ToString());
            text = text.Replace("#ru", ((char)139).ToString());
            text = text.Replace("#ld", ((char)140).ToString());
            text = text.Replace("#rd", ((char)141).ToString());

            text = text.Replace("#D", ((char)142).ToString());
            text = text.Replace("#lt", ((char)143).ToString());
            text = text.Replace("#rt", ((char)144).ToString());
            text = text.Replace("#Q", ((char)145).ToString());
            text = text.Replace("#E", ((char)146).ToString());

            text = text.Replace("#l", ((char)128).ToString());
            text = text.Replace("#r", ((char)129).ToString());
            text = text.Replace("#u", ((char)130).ToString());
            text = text.Replace("#d", ((char)131).ToString());

            text = text.Replace("#a", ((char)132).ToString());
            text = text.Replace("#b", ((char)133).ToString());
            text = text.Replace("#x", ((char)134).ToString());
            text = text.Replace("#y", ((char)135).ToString());

            text = text.Replace("#A", ((char)136).ToString());
            text = text.Replace("#S", ((char)137).ToString());

            text = text.Replace("\r\n", "\n");
            text = text.Replace("|\n", "|");
            text = text.Replace("\n|", "|");
            text = text.Replace("  ", " ");

            maxWidth = (int)((RoomCamera.Current.ViewWidth - Globals.T - 4) / zoom);

            foreach (var txt in text.Split('|'))
            {
                var textLines = txt.Split(new[] { '\n' });

                List<string> temporaryTexts = new List<string>();

                foreach (var textLine in textLines)
                {
                    var line = PrepareMessageString(textLine);
                    temporaryTexts.Add(line);
                }

                foreach(var groupedTextLines in GroupTextLinesToMessage(temporaryTexts))
                {
                    texts.Add(groupedTextLines);
                }                
            }
            //texts = GroupTextLinesToMessage(temporaryTexts);
            
            Depth = Globals.LAYER_FONT - .001f;

            Texture = AssetManager.MessageBox;

            this.hiColor = hiColor;
            this.textSpeed = textSpeed;
            this.showBorder = showBorder;

            Visible = true;
            
            state = MessageState.FADE_IN;
        }

        private List<string> GroupTextLinesToMessage(List<string> texts)
        {
            List<string> result = new List<string>();

            int lineCount = 0;
            var line = "";
            for (var i = 0; i < texts.Count; i++)
            {                
                if (lineCount < 3)
                {
                    line += texts[i] + ((lineCount < 2) ? "\n" : "");
                    lineCount++;
                }
                else
                {
                    result.Add(line);
                    line = texts[i] + "\n";
                    lineCount = 0;
                }
            }
            if (line != "")
            {
                result.Add(line.EndsWith("\n")? line.Substring(0, line.Length - 1) : line);
            }

            List<string> flat = new List<string>();

            foreach(var res in result)
            {
                foreach(var s in res.Split('\n'))
                {
                    flat.Add(s);
                }
            }
            
            var index = 0;
            var grpOf3s = flat.GroupBy(x => index++ / 3).ToList();

            result = new List<string>();
            var last = grpOf3s.Last();
            foreach(var group in grpOf3s)
            {
                var res = "";
                foreach (var g in group)
                {
                    res += g + "\n";
                }

                var cleanedLine = res.Substring(0, res.Length - 1).Replace(" \n", "\n").Replace("\n ", "\n");

                cleanedLine = cleanedLine.StartsWith(" ") ? cleanedLine.Substring(1) : cleanedLine;
                cleanedLine = cleanedLine.EndsWith(" ") ? cleanedLine.Substring(0, cleanedLine.Length - 1) : cleanedLine;

                result.Add(cleanedLine);
            }
            
            return result;
        }

        private string PrepareMessageString(string text)
        {
            var tmp = CutString(text, maxWidth);
            
            string wholeString = "";

            foreach (var t in tmp)
            {
                wholeString += t;
                wholeString += "\n";            
            }
            if (wholeString.EndsWith("\n"))
            {
                wholeString = wholeString.Substring(0, wholeString.Length - 1);
            }

            return wholeString;            
        }
        
        public override void EndUpdate(GameTime gameTime)
        {
            base.EndUpdate(gameTime);

            Position = new Vector2(RoomCamera.Current.ViewX, RoomCamera.Current.ViewY + offY);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            AppearDelay = Math.Max(AppearDelay - 1, 0);
            if (AppearDelay > 0)
            {
                return;
            }

            if (hiColor != null)
                font.HighlightColor = (Color)hiColor;

            kPrevPressed = InputMapping.KeyPressed(InputMapping.MessagePrev);
            kNextPressed = InputMapping.KeyPressed(InputMapping.MessageNext);

            kLeftPressed = InputMapping.KeyPressed(InputMapping.Left);
            kRightPressed = InputMapping.KeyPressed(InputMapping.Right);

            var kAny = InputMapping.IsAnyInputPressed();
            
            if (kNextPressed)
            {
                if (page == texts.Count - 1)
                {
                    if (curText.Length == texts[page].Length)
                    {
                        if (this is MessageDialog dialog && dialog.option == 0)
                        {
                            SoundManager.Play(AssetManager.MsgSelectNo);
                        }
                        else
                        {
                            SoundManager.Play(AssetManager.MsgSelectYes);
                        }

                        state = MessageState.FADE_OUT;
                        return;
                    }
                }
                if (curText.Length == texts[page].Length)
                {
                    if (page < texts.Count - 1)
                        curText = "";
                    else
                        curText = texts[page];

                    page = Math.Min(page + 1, texts.Count - 1);

                    //SoundManager.Play(AssetManager.MsgNextPage);
                }
            }
            else if (kAny)
            {
                if (textSpeed == TextSpeed.NORMAL)
                {
                    timeOut = 0;
                }
                else
                {
                    if (timeOut > 0)
                        timeOut--;
                }
            }

            soundTimeout = Math.Max(soundTimeout - 1, 0);
            timeOut = Math.Max(timeOut - 1, 0);
            if (timeOut == 0)
            {
                if (curText.Length < texts[page].Length)
                {
                    var newChar = texts[page].ElementAt(curText.Length);
                    curText += newChar;

                    var inColorFindingMode = newChar == '[';

                    while (inColorFindingMode && newChar != ']')
                    {
                        newChar = texts[page].ElementAt(curText.Length);
                        curText += newChar;                        
                    }

                    if (soundTimeout == 0 && newChar != ' ')
                    {
                        soundTimeout = 3;
                        SoundManager.Play(AssetManager.MsgChar);
                    }
                }
                
                timeOut = (int)textSpeed;
            }

            sin = (float)((sin + .1) % (2 * Math.PI));

            cursorAlpha = (curText.Length == texts[page].Length) ? Math.Min(cursorAlpha + .02f, 1) : 0;
        }

        public virtual void DrawActionIcons(SpriteBatch sb)
        {
            var T = Globals.T;
            var z = (float)Math.Sin(sin);

            int ca = (int)(((float)Color.A) * cursorAlpha);

            var cursorColor = new Color(Color, ca);
            
            if (page == 0 && texts.Count > 1)
            {
                sb.Draw(Texture, Position + new Vector2(RoomCamera.Current.ViewWidth - 2 * T, 2 * T + z), new Rectangle(1 * T, 3 * T, T, T), cursorColor, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, Depth + .001f);
            }
            else if (page < texts.Count - 1)
            {
                sb.Draw(Texture, Position + new Vector2(RoomCamera.Current.ViewWidth - 2 * T, 2 * T + z), new Rectangle(1 * T, 3 * T, T, T), cursorColor, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, Depth + .001f);
            }
            else
            {
                sb.Draw(Texture, Position + new Vector2(RoomCamera.Current.ViewWidth - 2 * T, 2 * T + z), new Rectangle(2 * T, 3 * T, T, T), cursorColor, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, Depth + .001f);
            }
        }
        
        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            //base.Draw(sb, gameTime);
            
            if (AppearDelay > 0)
            {
                return;
            }

            if (state == MessageState.FADE_IN)
            {
                alpha = Math.Min(alpha + .1f, 1);

                if (alpha == 1)
                    state = MessageState.SHOW;                
            }

            if (state == MessageState.FADE_OUT)
            {
                alpha = Math.Max(alpha - .1f, 0);
                if (alpha == 0)
                {
                    CompleteActionDelay = Math.Max(CompleteActionDelay - 1, 0);
                    if (CompleteActionDelay == 0)
                    {
                        OnCompleted?.Invoke();
                        OnCompleted = null;
                        Destroy();
                    }
                }
            }

            Color = new Color(Color, alpha);

            if (showBorder)
            {
                sb.Draw(Texture, Position, new Rectangle(0, 0, RoomCamera.Current.ViewWidth, 3 * Globals.T), Color, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, Depth);
            }

            DrawActionIcons(sb);

            float offX = 0 + Convert.ToInt32(font.Halign == Font.HorizontalAlignment.Center) * (RoomCamera.Current.ViewWidth * .5f - 10);

            font.Halign = halign;
            font.Valign = valign;
            font.Color = Color;
            
            if (hiColor != null)
                font.HighlightColor = new Color((Color)hiColor, alpha);

            if (!string.IsNullOrEmpty(curText) && alpha > .5f)
                font.Draw(sb, X + 8 + 2 + offX, Y + 8 + 2, curText, (int)(maxWidth / zoom), zoom);
        }
    }
}
