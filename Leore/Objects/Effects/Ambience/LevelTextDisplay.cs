﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leore.Main;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Draw;
using SPG.Objects;
using SPG.Util;

namespace Leore.Objects.Effects.Ambience
{
    public class LevelTextDisplay : GameObject, IKeepAliveBetweenRooms, IKeepEnabledAcrossRooms
    {
        private float alpha;
        private float xo;
        private bool shown;

        Font font;
        float fontZoom;
        RoomCamera camera => RoomCamera.Current;

        public LevelTextDisplay(float x, float y, string name = null) : base(x, y, name)
        {
            xo = camera.ViewWidth * .25f;

            if (Properties.Settings.Default.HighResText == true)
            {
                font = AssetManager.MessageFont.Copy();
                fontZoom = .5f;
                font.Spacing = (uint)(1 / fontZoom);
            }
            else
            {
                font = AssetManager.DefaultFont.Copy();
                fontZoom = 1;
                font.Spacing = 1;
            }

        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Position = camera.Position;

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

            font.Draw(sb, camera.ViewX + xo, camera.ViewY + 3 * Globals.T, Name, scale: fontZoom);

            float y = camera.ViewY + 3 * Globals.T + 5;
            var depth = font.Depth;

            sb.DrawRectangle(new RectF(camera.ViewX, y - 11, camera.ViewWidth + 1, 14), new Color(Color.Black, Math.Min(.5f * alpha, .5f)), true, depth - .0001f);
            sb.DrawLine(camera.ViewX, y, camera.ViewX + camera.ViewWidth, y, font.Color, depth);
        }
    }
}
