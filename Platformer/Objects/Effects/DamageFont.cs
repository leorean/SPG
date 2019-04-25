using Microsoft.Xna.Framework;
using SPG;
using SPG.Draw;
using SPG.Objects;
using SPG.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects.Effects
{
    class DamageFont : GameObject
    {
        string text;
        float alpha;

        Font font;

        public DamageFont(float x, float y, string text)
        {
            Position = new Vector2(x, y);
            this.text = text;

            alpha = 2f;
            YVel = -2f;

            font = MainGame.DefaultFont.Copy();

            font.Halign = Font.HorizontalAlignment.Center;
            font.Valign = Font.VerticalAlignment.Center;
            font.Color = Color.Red;

            XVel = -1f + (float)RND.Next * 2f;
            Gravity = .15f;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            YVel += Gravity;

            Move(XVel, YVel);
            alpha = Math.Max(alpha - .01f, 0);

            var c = font.Color;
            font.Color = new Color(c, alpha);

            if (alpha == 0 || Y > GameManager.Game.Camera.ViewY + GameManager.Game.Camera.ViewHeight)
                Destroy();
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            
            font.Draw(X, Y, text);
        }        
    }
}
