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
        
        private float offX, offY;
        private GameObject target;
        public GameObject Target { get => target; set { target = value; offX = (X - target.X); offY = (Y - target.Y); } }

        public DamageFont(float x, float y, string text)
        {
            Position = new Vector2(x, y);
            this.text = text;

            alpha = 1.5f;
            //YVel = -1f;

            font = MainGame.DamageFont.Copy();

            font.Halign = Font.HorizontalAlignment.Center;
            font.Valign = Font.VerticalAlignment.Center;
            font.Color = Color.Red;

            //XVel = -.5f + (float)RND.Next * 1f;
            //Gravity = .05f;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (ObjectManager.Exists(Target))
            {
                YVel = 0;
                Position = new Vector2(Target.X + offX, Target.Y + offY);
            }

            //YVel += Gravity;
            //Move(XVel, YVel);

            alpha = Math.Max(alpha - .02f, 0);

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
