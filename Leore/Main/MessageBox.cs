using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SPG;
using SPG.Draw;
using SPG.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Leore.Main
{
    public class MessageBox : GameObject
    {
        public enum TextSpeed
        {
            NORMAL = 3,
            SLOW = 10
        }

        private TextSpeed textSpeed;

        public Action OnCompleted;

        protected List<string> texts;
        protected Font font;
        protected int maxWidth;

        protected int offY = 6 * Globals.T;
        protected int page = 0;
        protected string curText = "";
        protected float sin = 0;

        protected Input input;
        private int timeOut;
        private Color? hiColor;
        protected Font.HorizontalAlignment halign;
        protected Font.VerticalAlignment valign;

        protected bool kLeftPressed, kRightPressed, kPrevPressed, kNextPressed;

        protected float alpha;

        public enum MessageState { FADE_IN, SHOW, FADE_OUT }
        protected MessageState state;

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
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="text"></param>
        /// <param name="name"></param>
        public MessageBox(string text, bool centerText = false, string name = null, Color? hiColor = null, TextSpeed textSpeed = TextSpeed.NORMAL) : base(0, 0, name)
        {
            input = new Input();

            texts = new List<string>();

            text = text.Replace("#lu", ((char)138).ToString());
            text = text.Replace("#ru", ((char)139).ToString());
            text = text.Replace("#ld", ((char)140).ToString());
            text = text.Replace("#rd", ((char)141).ToString());

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

            var split = text.Split('|');

            foreach (var t in split)
                texts.Add(t);
            
            Depth = Globals.LAYER_FONT - .001f;

            Texture = AssetManager.MessageBox;
            
            this.hiColor = hiColor;

            this.textSpeed = textSpeed;

            font = AssetManager.DefaultFont.Copy();
            halign = centerText ? Font.HorizontalAlignment.Center : Font.HorizontalAlignment.Left;
            valign = Font.VerticalAlignment.Top;

            Visible = true;

            maxWidth = RoomCamera.Current.ViewWidth - Globals.T - 4;

            state = MessageState.FADE_IN;
        }

        ~MessageBox()
        {
            input = null;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            input.Update(gameTime);

            if (hiColor != null)
                font.HighlightColor = (Color)hiColor;
            
            //kPrevPressed = input.IsKeyPressed(Keys.Up, Input.State.Pressed) || input.IsKeyPressed(Keys.S, Input.State.Pressed);
            //kNextPressed = input.IsKeyPressed(Keys.Down, Input.State.Pressed) || input.IsKeyPressed(Keys.A, Input.State.Pressed);
            kPrevPressed = input.IsKeyPressed(Keys.S, Input.State.Pressed);
            kNextPressed = input.IsKeyPressed(Keys.A, Input.State.Pressed);

            kLeftPressed = input.IsKeyPressed(Keys.Left, Input.State.Pressed) || input.DirectionPressedFromStick(Input.Direction.LEFT, Input.Stick.LeftStick, Input.State.Pressed);
            kRightPressed = input.IsKeyPressed(Keys.Right, Input.State.Pressed) || input.DirectionPressedFromStick(Input.Direction.RIGHT, Input.Stick.LeftStick, Input.State.Pressed);

            var kAny = input.IsAnyKeyPressed();

            if (input.GamePadEnabled)
            {
                //kPrevPressed = input.DirectionPressedFromStick(Input.Direction.UP, Input.Stick.LeftStick, Input.State.Pressed)
                //    || input.IsButtonPressed(Buttons.B);
                //kNextPressed = input.DirectionPressedFromStick(Input.Direction.DOWN, Input.Stick.LeftStick, Input.State.Pressed)
                //    || input.IsButtonPressed(Buttons.A);
                kPrevPressed = input.IsButtonPressed(Buttons.A);
                kNextPressed = input.IsButtonPressed(Buttons.B);
                kAny = input.IsAnyButtonPressed();
            }

            Position = new Vector2(RoomCamera.Current.ViewX, RoomCamera.Current.ViewY + offY);
            
            //if (kPrevPressed)
            //{
            //    if (page != 0)
            //        curText = "";

            //    page = Math.Max(page - 1, 0);
            //}
            if (kNextPressed)
            {
                if (page == texts.Count - 1)
                {
                    if (curText.Length == texts[page].Length)
                    {
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
                } else
                {
                    //curText = texts[page];
                    //timeOut = 0;
                }
            } else if (kAny)
            {
                if (textSpeed == TextSpeed.NORMAL)
                    timeOut = 0;
                else
                {
                    if (timeOut > 0)
                        timeOut --;
                }                
            }

            timeOut = Math.Max(timeOut - 1, 0);
            if (timeOut == 0)
            {
                if (curText.Length < texts[page].Length)
                {
                    var newChar = texts[page].ElementAt(curText.Length);
                    curText = curText + newChar;

                    var inColorFindingMode = newChar == '[';

                    while (inColorFindingMode && newChar != ']')
                    {
                        newChar = texts[page].ElementAt(curText.Length);
                        curText = curText + newChar;
                    }
                }

                timeOut = (int)textSpeed;                
            }
            
            sin = (float)((sin + .1) % (2 * Math.PI));            
        }

        public virtual void DrawActionIcons(SpriteBatch sb)
        {
            var T = Globals.T;
            var z = (float)Math.Sin(sin);

            if (page == 0 && texts.Count > 1)
            {
                sb.Draw(Texture, Position + new Vector2(RoomCamera.Current.ViewWidth - 2 * T, 2 * T + z), new Rectangle(1 * T, 3 * T, T, T), Color, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, Depth + .001f);
            }
            else if (page < texts.Count - 1)
            {
                //sb.Draw(Texture, Position + new Vector2(RoomCamera.Current.ViewWidth - 2 * T, 0 * T - z), new Rectangle(0 * T, 3 * T, T, T), Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, Depth + .001f);
                sb.Draw(Texture, Position + new Vector2(RoomCamera.Current.ViewWidth - 2 * T, 2 * T + z), new Rectangle(1 * T, 3 * T, T, T), Color, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, Depth + .001f);
            }
            else
            {
                sb.Draw(Texture, Position + new Vector2(RoomCamera.Current.ViewWidth - 2 * T, 2 * T + z), new Rectangle(2 * T, 3 * T, T, T), Color, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, Depth + .001f);
            }
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            //base.Draw(sb, gameTime);
            
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
                    OnCompleted?.Invoke();
                    OnCompleted = null;
                    Destroy();
                }
            }

            Color = new Color(Color, alpha);
            
            sb.Draw(Texture, Position, new Rectangle(0,0, RoomCamera.Current.ViewWidth, 3 * Globals.T), Color, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, Depth);

            DrawActionIcons(sb);

            float offX = 0 + Convert.ToInt32(font.Halign == Font.HorizontalAlignment.Center) * (RoomCamera.Current.ViewWidth * .5f - 10);

            font.Halign = halign;
            font.Valign = valign;
            font.Color = Color;

            if (hiColor != null)
                font.HighlightColor = new Color((Color)hiColor, alpha);

            if (!string.IsNullOrEmpty(curText))
                font.Draw(sb, X + 8 + 2 + offX, Y + 8 + 2, curText, maxWidth);
        }
    }
}
