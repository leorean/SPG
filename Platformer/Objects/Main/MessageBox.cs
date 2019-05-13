using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Platformer.Main;
using SPG;
using SPG.Draw;
using SPG.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects.Main
{
    public class MessageBox : GameObject
    {
        List<string> texts;
        Font font;
        int maxWidth;

        int offY = 6 * Globals.TILE;
        int page = 0;
        string curText = "";
        float sin = 0;

        Input input;

        int timeOut;

        public Action OnCompleted;

        /// <summary>
        /// Creates a new message box.
        /// 
        /// Special chars:
        /// \n ... new line
        /// | ... new page
        /// ~ ... toggle highlighting (example: ~word~)
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
        /// #Y ... Y key
        /// #X ... X key
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="text"></param>
        /// <param name="name"></param>
        public MessageBox(string text, bool centerText = false, string name = null) : base(0, 0, name)
        {
            input = new Input();

            texts = new List<string>();

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

            font = AssetManager.DefaultFont.Copy();
            font.Halign = centerText ? Font.HorizontalAlignment.Center : Font.HorizontalAlignment.Left;
            font.Valign = Font.VerticalAlignment.Top;

            maxWidth = RoomCamera.Current.ViewWidth - Globals.TILE - 4;
        }

        ~MessageBox()
        {
            input = null;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            input.Update(gameTime);

            var kUpPressed = input.IsKeyPressed(Keys.Up, Input.State.Pressed) || input.IsKeyPressed(Keys.S, Input.State.Pressed);
            var kDownPressed = input.IsKeyPressed(Keys.Down, Input.State.Pressed) || input.IsKeyPressed(Keys.A, Input.State.Pressed);

            var kAny = input.IsAnyKeyPressed();

            if (input.GamePadEnabled)
            {
                kUpPressed = input.DirectionPressedFromStick(Input.Direction.UP, Input.Stick.LeftStick, Input.State.Pressed)
                    || input.IsButtonPressed(Buttons.B);
                kDownPressed = input.DirectionPressedFromStick(Input.Direction.DOWN, Input.Stick.LeftStick, Input.State.Pressed)
                    || input.IsButtonPressed(Buttons.A);
                kAny = input.IsAnyButtonPressed();
            }

            Position = new Vector2(RoomCamera.Current.ViewX, RoomCamera.Current.ViewY + offY);
            
            if (kUpPressed)
            {
                if (page != 0)
                    curText = "";

                page = Math.Max(page - 1, 0);
            }
            else if (kDownPressed)
            {
                if (page == texts.Count - 1)
                {
                    if (curText.Length == texts[page].Length)
                    {
                        OnCompleted?.Invoke();
                        OnCompleted = null;
                        Destroy();
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
                timeOut = 0;
            }

            timeOut = Math.Max(timeOut - 1, 0);
            if (timeOut == 0)
            {
                if (curText.Length < texts[page].Length)
                {
                    curText = curText + texts[page].ElementAt(curText.Length);
                }

                timeOut = 3;
            }
            
            sin = (float)((sin + .1) % (2 * Math.PI));
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            //base.Draw(sb, gameTime);

            var T = Globals.TILE;
            var z = (float)Math.Sin(sin);

            sb.Draw(Texture, Position, new Rectangle(0,0, RoomCamera.Current.ViewWidth, 3 * Globals.TILE), Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, Depth);
            
            if (page == 0 && texts.Count > 1)
            {
                sb.Draw(Texture, Position + new Vector2(RoomCamera.Current.ViewWidth - 2 * T, 2 * T + z), new Rectangle(1 * T, 3 * T, T, T), Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, Depth + .001f);
            }
            else if (page < texts.Count - 1)
            {
                sb.Draw(Texture, Position + new Vector2(RoomCamera.Current.ViewWidth - 2 * T, 0 * T - z), new Rectangle(0 * T, 3 * T, T, T), Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, Depth + .001f);
                sb.Draw(Texture, Position + new Vector2(RoomCamera.Current.ViewWidth - 2 * T, 2 * T + z), new Rectangle(1 * T, 3 * T, T, T), Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, Depth + .001f);
            } else
            {
                sb.Draw(Texture, Position + new Vector2(RoomCamera.Current.ViewWidth - 2 * T, 2 * T + z), new Rectangle(2 * T, 3 * T, T, T), Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, Depth + .001f);
            }

            float offX = 0 + Convert.ToInt32(font.Halign == Font.HorizontalAlignment.Center) * (RoomCamera.Current.ViewWidth * .5f - 10);

            if (!string.IsNullOrEmpty(curText))
                font.Draw(sb, X + 8 + 2 + offX, Y + 8 + 2, curText, maxWidth);
        }
    }
}
