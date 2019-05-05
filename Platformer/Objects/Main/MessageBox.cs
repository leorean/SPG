using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Platformer.Main;
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
        string text;
        Font font;
        int maxWidth;

        int offY = 5 * Globals.TILE;

        public MessageBox(float x, float y, string text, string name = null) : base(x, y, name)
        {
            this.text = text;
            Depth = Globals.LAYER_FONT - .001f;

            Texture = AssetManager.MessageBoxSprite;

            font = AssetManager.DefaultFont.Copy();
            font.Halign = Font.HorizontalAlignment.Left;
            font.Valign = Font.VerticalAlignment.Top;

            maxWidth = RoomCamera.Current.ViewWidth - Globals.TILE - 4;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Position = new Vector2(RoomCamera.Current.ViewX, RoomCamera.Current.ViewY + offY);
            
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);

            font.Draw(sb, X + 8 + 2, Y + 8, text, maxWidth);
        }
    }
}
