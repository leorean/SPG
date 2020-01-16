using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Draw;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leore.Main
{
    public class TitleMenu
    {
        private RoomCamera camera => RoomCamera.Current;
        Vector2 position;

        private Font font = AssetManager.HUDFont;

        double t = 0;
        float z = 0;
        
        public TitleMenu()
        {

        }

        public void Update(GameTime gameTime)
        {
            position = new Vector2(camera.ViewX, camera.ViewY);

            t = (t + .025f);

            z = (float)(2 * Math.Sin(t));
        }

        public void Draw(SpriteBatch sb, GameTime gameTime)
        {
            // BG
            sb.Draw(AssetManager.TitleMenu, position, new Rectangle(256, 0, 256, 144), Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0.0000f);

            // floaty thing
            sb.Draw(AssetManager.TitleMenu, position + new Vector2(0, z), new Rectangle(0, 0, 256, 144), Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0.0001f);

            // font
            font.Halign = Font.HorizontalAlignment.Left;
            font.Valign = Font.VerticalAlignment.Top;
            font.Draw(sb, position.X + 25, position.Y + 144 - 32, $"TEST", depth: .00003f);


        }
    }
}
