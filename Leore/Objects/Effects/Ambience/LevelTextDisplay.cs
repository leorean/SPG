using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leore.Main;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Draw;
using SPG.Util;

namespace Leore.Objects.Effects.Ambience
{
    public class LevelTextDisplay : RoomObject
    {
        private float alpha;
        private float xo;
        private bool shown;

        Font font = AssetManager.DefaultFont.Copy();
        RoomCamera camera => RoomCamera.Current;

        public LevelTextDisplay(float x, float y, Room room, string name = null) : base(x, y, room, name)
        {
            xo = camera.ViewWidth * .25f;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!shown)
            {
                var maxX = .5f * camera.ViewWidth;
                var maxAlpha = 1.5f;

                xo = Math.Min(xo + 2, maxX);
                alpha = Math.Min(alpha + .01f, maxAlpha);

                if (alpha == maxAlpha && xo == maxX)
                    shown = true;
            }
            else
            {
                alpha = Math.Max(alpha - .02f, 0);
                if (alpha < .5f)
                    xo += 3;

                if (alpha == 0)
                {
                    Destroy();
                }
            }         
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            //base.Draw(sb, gameTime);

            font.Halign = Font.HorizontalAlignment.Center;
            font.Valign = Font.VerticalAlignment.Center;

            font.Color = new Color(Color, alpha);

            font.Draw(sb, camera.ViewX + xo, camera.ViewY + 3 * Globals.T, Name);

            var y = camera.ViewY + 3 * Globals.T + 4;
            var depth = font.Depth;

            sb.DrawRectangle(new RectF(camera.ViewX, y - 10, camera.ViewWidth + 1, 13), new Color(Color.Black, .5f * alpha), true, depth - .0001f);
            sb.DrawLine(camera.ViewX, y, camera.ViewX + camera.ViewWidth, y, font.Color, depth);            

        }
    }
}
