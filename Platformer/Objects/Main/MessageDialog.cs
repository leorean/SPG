using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Platformer.Main;
using SPG;

namespace Platformer.Objects.Main
{
    public class MessageDialog : MessageBox
    {
        public Action YesAction;
        public Action NoAction;

        int option = 1;

        public MessageDialog(string text, bool centerText = false, string name = null, Color? hiColor = null) : base(text, centerText, name, hiColor)
        {
            /*if (text.Contains('|'))
                throw new Exception($"MessageDialog with ID {ID} cannot have a multi-page separator!");
            */
            OnCompleted = OnCompletedAction;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            if (option == 1 && kRightPressed)
                option = 0;

            if (option == 0 && kLeftPressed)
                option = 1;
        }

        public override void DrawActionIcons(SpriteBatch sb)
        {
            //base.DrawActionIcons(sb);
            var T = Globals.TILE;

            font.Halign = SPG.Draw.Font.HorizontalAlignment.Center;
            font.Valign = SPG.Draw.Font.VerticalAlignment.Center;

            if (curText.Length == texts[page].Length && page == texts.Count - 1)
            {
                // yes
                Vector2 yesPos = Position + new Vector2(RoomCamera.Current.ViewWidth - 5 * T, -.5f * T);
                sb.Draw(Texture, yesPos, new Rectangle(3 * T, 3 * T, 2 * T, T), Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, Depth - .001f);
                if (option == 1) sb.Draw(Texture, yesPos, new Rectangle(5 * T, 3 * T, 2 * T, T), Color, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, Depth);

                //no
                Vector2 noPos = Position + new Vector2(RoomCamera.Current.ViewWidth - 3 * T, -.5f * T);
                sb.Draw(Texture, noPos, new Rectangle(3 * T, 3 * T, 2 * T, T), Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, Depth - .001f);
                if (option == 0) sb.Draw(Texture, noPos, new Rectangle(5 * T, 3 * T, 2 * T, T), Color, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, Depth);

                // cursor
                var curPos = new Vector2(-T + (float)Math.Sin(sin) * 2, 0);
                sb.Draw(Texture, option == 1 ? yesPos + curPos : noPos + curPos, new Rectangle(7 * T, 3 * T, 2 * T, T), Color, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, Depth + .001f);

                font.Color = Color;

                font.Draw(sb, yesPos.X + T, yesPos.Y + 8, "Yes");
                font.Draw(sb, noPos.X + T, noPos.Y + 8, "No");

            }

            font.Halign = halign;
            font.Valign = valign;
        }

        public void OnCompletedAction()
        {
            if (option == 1)
                YesAction?.Invoke();
            if (option == 0)
                NoAction?.Invoke();
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);
        }
    }
}
